using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IPresenceTracker _presenceTracker;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
            IPhotoService photoService, IPresenceTracker presenceTracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
            _presenceTracker = presenceTracker;
        }

        [HttpGet]
        public async Task<IList<UserDto>> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserName = User.GetUserName();
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                var currentUser = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: true);
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetUsersAsync(userParams.CurrentPage, userParams.PageSize, currentUserName,
                userParams.Gender, userParams.MinAge, userParams.MaxAge, userParams.OrderBy, onlyGetApprovedPhotos: true);

            Response.AddPaginationHeader(users);
            var usersDto = _mapper.Map<List<UserDto>>(users);
            usersDto.ForEach(u => u.IsOnline = _presenceTracker.IsOnline(u.UserName));

            return usersDto;
        }

        [HttpGet("for-edit")]
        public async Task<UserDto> GetUserForEdit()
        {
            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: false);
            return MapToUserDto(user);
        }

        [HttpGet("{userName}", Name = "GetUser")]
        public async Task<UserDto> GetUser(string userName)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(userName, onlyGetApprovedPhotos: true);
            return MapToUserDto(user);
        }

        private UserDto MapToUserDto(AppUser user)
        {
            var userDto = _mapper.Map<UserDto>(user);
            userDto.IsOnline = _presenceTracker.IsOnline(user.UserName);
            return userDto;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: false);

            _mapper.Map(userUpdateDto, user);
            _unitOfWork.UserRepository.Update(user);

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to update user!");

            return NoContent();
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);
            if (string.IsNullOrEmpty(uploadResult.PublicId)) return BadRequest("Failed to upload photo");

            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: false);
            var photo = new Photo
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.AbsoluteUri,
                IsMain = false,
                IsApproved = false
            };
            user.Photos.Add(photo);

            if (!await _unitOfWork.Complete()) return BadRequest("Problem adding photo!");

            return CreatedAtRoute("GetUser", new { user.UserName }, _mapper.Map<PhotoDto>(photo));
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: false);
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return BadRequest("Photo does not exist!");
            if (!photo.IsApproved) return BadRequest("Photo is wating for approval, can't set it to main photo!");
            if (photo.IsMain) return BadRequest("This is already your main photo!");

            foreach (var userPhoto in user.Photos)
                if (userPhoto.IsMain)
                {
                    userPhoto.IsMain = false;
                }
            photo.IsMain = true;

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to set main photo!");

            return NoContent();
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User, onlyGetApprovedPhotos: false);
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

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to delete photo!");

            return Ok();
        }
    }
}
