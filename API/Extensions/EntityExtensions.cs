using API.Entities;

namespace API.Extensions
{
    public static class EntityExtensions
    {
        public static bool IsMainPhoto(this Photo photo)
        {
            return photo.IsMain && photo.IsApproved;
        }
    }
}
