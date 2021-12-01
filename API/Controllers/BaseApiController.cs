using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected async Task<AppUser> GetCurrentUser(IUserRepository userRepository)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await userRepository.GetUserByUserNameAsync(userName);
        }
    }
}
