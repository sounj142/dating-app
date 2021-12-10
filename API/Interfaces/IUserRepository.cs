using API.Entities;
using API.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Add(AppUser user);
        void Update(AppUser user);
        Task<PagedList<AppUser>> GetUsersAsync(int currentPage, int pageSize, string currentUserName, 
            string gender, int? minAge, int? maxAge, string orderBy, bool onlyGetApprovedPhotos);
        Task<AppUser> GetUserByIdAsync(int id, bool onlyGetApprovedPhotos);
        Task<AppUser> GetUserByUserNameAsync(string userName, bool onlyGetApprovedPhotos);
        Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal, bool onlyGetApprovedPhotos);
    }
}
