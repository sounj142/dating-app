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
            string gender, int? minAge, int? maxAge, string orderBy);
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUserNameAsync(string userName);
        Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal);
    }
}
