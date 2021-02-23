﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Util;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoUploadService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoUploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoUploadService = photoUploadService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUserName = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender.ToUpper() == "MALE" ? "female" : "male";
            
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
                
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            return Ok(users);
        }
        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _unitOfWork.UserRepository.GetMemberAsync(username, User.GetUsername() == username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            
            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete())
                return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);

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

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                return CreatedAtRoute("GetUser",new {username = user.UserName} ,_mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain)
            {
                return BadRequest("This is already you main photo");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _unitOfWork.Complete())
                return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());

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

            if (await _unitOfWork.Complete())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
        
     }
}
