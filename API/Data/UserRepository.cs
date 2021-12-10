using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;

        private IQueryable<AppUser> UsersIncludedProps(bool onlyGetApprovedPhotos)
        {
            return onlyGetApprovedPhotos
                ? _dataContext.Users.Include(u => u.Photos.Where(p => p.IsApproved))
                : _dataContext.Users.Include(u => u.Photos);
        }

        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<AppUser> GetUserByIdAsync(int id, bool onlyGetApprovedPhotos)
        {
            return await UsersIncludedProps(onlyGetApprovedPhotos).FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName, bool onlyGetApprovedPhotos)
        {
            return await UsersIncludedProps(onlyGetApprovedPhotos).FirstOrDefaultAsync(user => user.UserName == userName);
        }

        public async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal, bool onlyGetApprovedPhotos)
        {
            var userId = claimsPrincipal.GetUserId();
            return await GetUserByIdAsync(userId, onlyGetApprovedPhotos);
        }

        public async Task<PagedList<AppUser>> GetUsersAsync(int currentPage, int pageSize, string currentUserName,
            string gender, int? minAge, int? maxAge, string orderBy, bool onlyGetApprovedPhotos)
        {
            var query = UsersIncludedProps(onlyGetApprovedPhotos)
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

        public void Update(AppUser user)
        {
            _dataContext.Entry(user).State = EntityState.Modified;
        }

        public void Add(AppUser user)
        {
            _dataContext.Users.Add(user);
        }

        public async Task<IList<PhotoForAdminFeatureDto>> GetPhotosToModerates()
        {
            return await _dataContext.Set<Photo>()
                .Where(p => !p.IsApproved)
                .OrderBy(p => p.AppUser.UserName)
                .Select(p => new PhotoForAdminFeatureDto
                {
                    Id = p.Id,
                    KnownAs = p.AppUser.KnownAs,
                    Url = p.Url,
                    UserId = p.AppUser.Id,
                    UserName = p.AppUser.UserName
                })
                .ToListAsync();
        }
    }
}
