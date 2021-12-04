using API.DTOs;
using API.Entities;
using API.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal);
    }
}
