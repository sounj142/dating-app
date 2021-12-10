using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.DTOs.Admins;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IPresenceTracker _presenceTracker;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public AdminController(DataContext dataContext, UserManager<AppUser> userManager, 
            IUnitOfWork unitOfWork, IPhotoService photoService, IPresenceTracker presenceTracker,
            IHubContext<PresenceHub> presenceHub)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _presenceTracker = presenceTracker;
            _presenceHub = presenceHub;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<IList<UserAndRolesInfoDto>> GetUsersWithRoles()
        {
            var userRoleInfo = await _userManager.Users
                .Select(user => new UserAndRolesInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    KnownAs = user.KnownAs,
                })
                .OrderBy(u => u.UserName)
                .AsNoTracking()
                .ToListAsync();

            var userRoleMaps = await (from userRole in _dataContext.UserRoles
                                      join role in _dataContext.Roles
                                          on userRole.RoleId equals role.Id
                                      orderby role.Name
                                      select new
                                      {
                                          userRole.UserId,
                                          RoleName = role.Name
                                      })
                                      .AsNoTracking()
                                      .ToListAsync();

            var userRoleInfoDict = userRoleInfo
                .ToDictionary(x => x.Id, x => {
                    x.Roles = new List<string>();
                    return x;
                });

            foreach(var mapItem in userRoleMaps)
            {
                userRoleInfoDict[mapItem.UserId].Roles.Add(mapItem.RoleName);
            }

            return userRoleInfo;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("edit-roles")]
        public async Task<ActionResult> EditRoles(EditRolesDto editRolesDto)
        {
            var user = await _userManager.FindByNameAsync(editRolesDto.UserName);

            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var newRoles = editRolesDto.Roles;

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(newRoles));
            if (!removeResult.Succeeded)
            {
                return BadRequest("Failed to assign user to role(s)");
            }

            var addResult = await _userManager.AddToRolesAsync(user, newRoles.Except(currentRoles));
            if (!addResult.Succeeded)
            {
                return BadRequest("Failed to assign user to role(s)");
            }

            return Ok();
        }

        [Authorize(Policy = "RequireModerateRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<IList<PhotoForAdminFeatureDto>> GetPhotosToModerates()
        {
            return await _unitOfWork.PhotoRepository.GetPhotosToModerates();
        }

        [Authorize(Policy = "RequireModerateRole")]
        [HttpPut("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound();
            if (photo.IsApproved) return BadRequest("Photo has been already approved");

            // set main photo if needed
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(photo.AppUserId, onlyGetApprovedPhotos: false);
            photo.IsMain = !user.Photos.Any(p => p.IsMainPhoto());
            photo.IsApproved = true;

            if (photo.IsMain)
            {
                var recipientConnections = _presenceTracker.GetConnections(user.UserName);
                if (recipientConnections.Any())
                {
                    await _presenceHub.Clients.Clients(recipientConnections).SendAsync("UserInfoChanged",
                        new UserUpdateNoticationData(photoUrl: photo.Url, knownAs: user.KnownAs, gender: user.Gender));
                }
            }

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to approve photo!");

            return Ok();
        }

        [Authorize(Policy = "RequireModerateRole")]
        [HttpPut("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound();
            if (photo.IsApproved) return BadRequest("Photo has been already approved");

            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                var deleteResult = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (deleteResult.Error != null)
                {
                    return BadRequest(deleteResult.Error.Message);
                }
            }

            _unitOfWork.PhotoRepository.DeletePhoto(photo);

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to delete photo!");

            return Ok();
        }
    }
}
