using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivityActionFilter : IAsyncActionFilter
    {
        private readonly ClientInformation _clientInformation;

        public LogUserActivityActionFilter(ClientInformation clientInformation)
        {
            _clientInformation = clientInformation;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var excutedContext = await next.Invoke();

            if (!excutedContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userRepository = excutedContext.HttpContext.RequestServices.GetService<IUserRepository>();

            var user = await userRepository.GetCurrentUserAsync(excutedContext.HttpContext.User);
            user.LastActive = _clientInformation.GetClientNow();
            userRepository.Update(user);

            await userRepository.SaveAllAsync();
        }
    }
}
