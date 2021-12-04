using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{userName}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUser = await _likesRepository.GetCurrentUserAsync(User);
            var likedUser = await _userRepository.GetUserByUserNameAsync(userName);

            if (likedUser == null)
                return NotFound();
            if (sourceUser.Id == likedUser.Id)
                return BadRequest("You cannot like yourself");
            if (sourceUser.LikedUsers.Any(x => x.LikedUserId == likedUser.Id))
                return BadRequest("You already like this user");

            sourceUser.LikedUsers.Add(new UserLike {
                SourceUserId = sourceUser.Id,
                LikedUserId = likedUser.Id
            });

            if (!await _userRepository.SaveAllAsync()) return BadRequest("Failed to like user");

            return Ok();
        }

        [HttpGet]
        public async Task<IList<LikeDto>> GetUserLikess([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var likes = await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(likes);
            return likes;
        }
    }
}
