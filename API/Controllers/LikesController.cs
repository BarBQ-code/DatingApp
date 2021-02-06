using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Exstensions;
using API.Interfaces;
using API.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesReposirtory _likesReposirtory;

        public LikesController(IUserRepository userRepository, ILikesReposirtory likesReposirtory)
        {
            _userRepository = userRepository;
            _likesReposirtory = likesReposirtory;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetId();
            var likedUser = await _userRepository.GetUserByUserNameAsync(username);

            var sourceUser = await _likesReposirtory.GetUserWithLikes(sourceUserId);

            if (likedUser == null)
                return NotFound();

            if (sourceUser.UserName == username)
                return BadRequest("You cannot like yourself");

            var userLike = await _likesReposirtory.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null)
                return BadRequest("You already liked this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };
            
            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync())
                return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetId();
            var users =  await _likesReposirtory.GetUserLikes(likesParams);
            
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            return Ok(users);
        }
    }
}