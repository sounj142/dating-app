using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
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
            return await UsersIncludedProps.FirstOrDefaultAsync(user => user.UserName == userName);
        }

        public async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            return await GetUserByIdAsync(userId);
        }

        public async Task<PagedList<AppUser>> GetUsersAsync(int currentPage, int pageSize, string currentUserName, 
            string gender, int? minAge, int? maxAge, string orderBy)
        {
            var query = UsersIncludedProps
                .Where(u => u.UserName != currentUserName)
                .Where(u => u.Gender == gender);

            var today = DateTimeExtensions.TodayStandardTimezone();
            if (minAge.HasValue)
            {
                var maxDob = today.AddYears(-minAge.Value);
                query = query.Where(user => user.DateOfBirth <= maxDob);
            }
            if (maxAge.HasValue)
            {
                var minDob = today.AddYears(-maxAge.Value - 1);
                query = query.Where(user => user.DateOfBirth >= minDob);
            }

            query = orderBy switch
            {
                "Created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<AppUser>.CreateAsync(query.AsNoTracking(), currentPage, pageSize);
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
