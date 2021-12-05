using API.Helpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API.Middlewares
{
    public class DetectClientInformationMiddleware
    {
        private readonly RequestDelegate _next;

        public DetectClientInformationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, ClientInformation clientInformation)
        {
            var clientTimeZoneValue = httpContext.Request.Headers["ClientTimeZoneOffset"];
            if (clientTimeZoneValue.Count == 1)
            {
                if (int.TryParse(clientTimeZoneValue[0], out int clientTimezoneOffset))
                    clientInformation.SetTimeZoneOffset(clientTimezoneOffset);
            }

            await _next(httpContext);
        }
    }
}
