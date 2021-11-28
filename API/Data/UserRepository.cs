using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;

        private IQueryable<AppUser> UsersIncludedProps => _dataContext.Users.Include(u => u.Photos);

        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await UsersIncludedProps.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            var userNameLowerCase = userName.ToLower();
            return await UsersIncludedProps.FirstOrDefaultAsync(user => user.UserName.ToLower() == userNameLowerCase);
        }

        public async Task<IList<AppUser>> GetUsersAsync()
        {
            return await UsersIncludedProps.ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _dataContext.Entry(user).State = EntityState.Modified;
        }  
        
        public void Add(AppUser user)
        {
            _dataContext.Users.Add(user);
        }
    }
}
