﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Exstensions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoUploadService _photoUploadService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoUploadService photoUploadService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoUploadService = photoUploadService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();

            return Ok(users);
        }
        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUserNameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            
            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUserNameAsync(username);

            var result = await _photoUploadService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            
            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser",new {username = user.UserName} ,_mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain)
            {
                return BadRequest("This is already you main photo");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
                return NotFound();

            if (photo.IsMain)
                return BadRequest("You can't delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoUploadService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) 
                    return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
        
     }
}
