using API.DTOs;
using API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IList<PhotoForAdminFeatureDto>> GetPhotosToModerates();
        Task<Photo> GetPhotoById(int id);
        void DeletePhoto(Photo photo);
    }
}
