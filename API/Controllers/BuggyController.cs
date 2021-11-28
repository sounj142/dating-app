using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }


        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            return NotFound();
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            AppUser o = null;
            return o.ToString();
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Hey, not good request!");
        }
    }
}
