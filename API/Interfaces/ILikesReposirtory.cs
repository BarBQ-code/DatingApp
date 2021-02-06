﻿using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Exstensions;
using API.Util;

namespace API.Interfaces
{
    public interface ILikesReposirtory
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}