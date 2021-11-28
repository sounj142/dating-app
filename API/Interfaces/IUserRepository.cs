using API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Add(AppUser user);
        void Update(AppUser user);
        Task<bool> SaveAllAsync();
        Task<IList<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUserNameAsync(string userName);
    }
}
