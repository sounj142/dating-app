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

            var unitOfWork = excutedContext.HttpContext.RequestServices.GetService<IUnitOfWork>();

            var user = await unitOfWork.UserRepository.GetCurrentUserAsync(excutedContext.HttpContext.User);
            user.LastActive = _clientInformation.GetClientNow();
            unitOfWork.UserRepository.Update(user);

            await unitOfWork.Complete();
        }
    }
}
