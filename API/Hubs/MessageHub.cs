﻿using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Exstensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, 
            IUserRepository userRepository, IHubContext<PresenceHub> presenceHub
            ,PresenceTracker tracker)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(httpContext.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            
            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            //automatically removes the user from his group
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username.ToUpper() == createMessageDto.RecipientUsername.ToUpper())
            {
                throw new HubException("You cannot send messages to yourself");
            }

            var sender = await _userRepository.GetUserByUserNameAsync(username);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null)
            {
                throw new HubException("Not found user");
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knowAs = sender.KnownAs
                    });
                }
            }
            
            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetMessageGroup(groupName);

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            group.Connections.Add(connection);

            if (await _messageRepository.SaveAllAsync())
            {
                return group;
            }

            throw new HubException("Failed to join the group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);

            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            
            _messageRepository.RemoveConnection(connection);

            if (await _messageRepository.SaveAllAsync())
            {
                return group;
            }
            throw new HubException("Failed to remove from group");
        }
        
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}