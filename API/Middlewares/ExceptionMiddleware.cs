using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment environment
            )
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                var statusCode = (int)HttpStatusCode.InternalServerError;
                var exceptionResponse = new ApiExceptionResponse(
                    statusCode: statusCode, 
                    message: "Internal Server Error", 
                    details: _environment.IsDevelopment() ? $"{ex.Message} {ex.StackTrace}" : null
                );

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var responseString = JsonSerializer.Serialize(exceptionResponse, options);

                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = "application/json; charset=utf-8";

                await httpContext.Response.WriteAsync(responseString);
            }
        }
    }
}
