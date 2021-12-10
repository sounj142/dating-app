using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _dataContext;

        public PhotoRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<PhotoForAdminFeatureDto>> GetPhotosToModerates()
        {
            return await _dataContext.Photos
                .Where(p => !p.IsApproved)
                
                .Select(p => new PhotoForAdminFeatureDto
                {
                    Id = p.Id,
                    KnownAs = p.AppUser.KnownAs,
                    Url = p.Url,
                    UserId = p.AppUser.Id,
                    UserName = p.AppUser.UserName
                })
                .OrderBy(p => p.UserName)
                .ToListAsync();
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await _dataContext.Photos.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void DeletePhoto(Photo photo)
        {
            _dataContext.Photos.Remove(photo);
        }
    }
}
