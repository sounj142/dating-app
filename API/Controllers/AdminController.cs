using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs.Admins;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(DataContext dataContext, UserManager<AppUser> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
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
        public ActionResult GetPhotosToModerates()
        {
            return Ok("Moderate and admin can see it!");
        }
    }
}
