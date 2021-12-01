using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<IList<UserDto>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();
            return _mapper.Map<IList<UserDto>>(users);
        }

        [HttpGet("{userName}", Name = "GetUser")]
        public async Task<UserDto> GetUser(string userName)
        {
            var user = await _userRepository.GetUserByUserNameAsync(userName);
            return _mapper.Map<UserDto>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var user = await GetCurrentUser(_userRepository);

            _mapper.Map(userUpdateDto, user);
            _userRepository.Update(user);

            if (!await _userRepository.SaveAllAsync()) return BadRequest("Failed to update user!");

            return NoContent();
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);
            if (string.IsNullOrEmpty(uploadResult.PublicId)) return BadRequest("Failed to upload photo");

            var user = await GetCurrentUser(_userRepository);
            var photo = new Photo
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.AbsoluteUri,
                IsMain = user.Photos.Count == 0
            };
            user.Photos.Add(photo);

            if (!await _userRepository.SaveAllAsync()) return BadRequest("Problem adding photo!");

            return CreatedAtRoute("GetUser", new { user.UserName }, _mapper.Map<PhotoDto>(photo));
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await GetCurrentUser(_userRepository);
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return BadRequest("Photo does not exist!");
            if (photo.IsMain) return BadRequest("This is already your main photo!");

            foreach (var userPhoto in user.Photos)
                if (userPhoto.IsMain)
                {
                    userPhoto.IsMain = false;
                }
            photo.IsMain = true;

            if (!await _userRepository.SaveAllAsync()) return BadRequest("Failed to set main photo!");

            return NoContent();
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await GetCurrentUser(_userRepository);
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound("Photo does not exist!");
            if (photo.IsMain) return BadRequest("Can't delete main photo!");

            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                var deleteResult = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (deleteResult.Error != null)
                {
                    return BadRequest(deleteResult.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if (!await _userRepository.SaveAllAsync()) return BadRequest("Failed to delete photo!");

            return Ok();
        }
    }
}
