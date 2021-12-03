using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivityActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var excutedContext = await next.Invoke();

            if (!excutedContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userRepository = excutedContext.HttpContext.RequestServices.GetService<IUserRepository>();

            var user = await userRepository.GetCurrentUserAsync(excutedContext.HttpContext.User);
            user.LastActive = DateTimeOffset.Now;
            userRepository.Update(user);

            await userRepository.SaveAllAsync();
        }
    }
}
