using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public LikesRepository(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _dataContext.Likes.FirstOrDefaultAsync(x =>
                x.SourceUserId == sourceUserId && x.LikedUserId == likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            IQueryable<AppUser> query;
            if (likesParams.Predicate == "Liked")
            {
                query = from user in _dataContext.Users
                        join like in _dataContext.Likes
                            on user.Id equals like.LikedUserId
                        where like.SourceUserId == likesParams.UserId
                        select user;
            }
            else
            {
                query = from user in _dataContext.Users
                        join like in _dataContext.Likes
                            on user.Id equals like.SourceUserId
                        where like.LikedUserId == likesParams.UserId
                        select user;
            }

            var likesQuery = query
                .ProjectTo<LikeDto>(_mapper.ConfigurationProvider)
                .OrderBy(u => u.UserName)
                .AsNoTracking();

            return await PagedList<LikeDto>.CreateAsync(likesQuery, likesParams.CurrentPage, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _dataContext.Users
                .Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            return await GetUserWithLikes(userId);
        }
    }
}
