using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using pos_service.Exceptions;

namespace pos_service.Middlewares
{
    /// <summary>
    /// Middleware to catch unhandled exceptions and return structured JSON responses.
    /// Maps common exception types to appropriate HTTP status codes.
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            object payload = new { error = "An unexpected error occurred." };

            switch (exception)
            {
                case PermissionDeniedException pde:
                    code = HttpStatusCode.Forbidden;
                    payload = new { error = pde.Message };
                    break;
                case UnauthorizedAccessException ua:
                    code = HttpStatusCode.Unauthorized;
                    payload = new { error = ua.Message };
                    break;
                case ArgumentException arg:
                    code = HttpStatusCode.BadRequest;
                    payload = new { error = arg.Message };
                    break;
                case KeyNotFoundException knf:
                    code = HttpStatusCode.NotFound;
                    payload = new { error = knf.Message };
                    break;
                default:
                    payload = new { error = exception.Message };
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            var json = JsonSerializer.Serialize(payload);
            return context.Response.WriteAsync(json);
        }
    }

    public static class GlobalExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionHandler>();
    }
}
