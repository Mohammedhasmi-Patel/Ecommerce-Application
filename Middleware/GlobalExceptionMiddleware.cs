using Ecommerce.Common;
using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using Ecommerce.Common.Exceptions;

namespace Ecommerce.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,AppDbContext _dbContext)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    var errorLog = new ErrorLog
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        RequestPath = context.Request.Path
                    };

                    await _dbContext.ErrorLogs.AddAsync(errorLog);
                    await HandleExceptionAsync(context, ex);
                }
                catch (Exception logException)
                {
                    Console.WriteLine(logException.ToString());
                }
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            var statusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                ConflictException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = statusCode == 500 ? "An internal server error occurred." : exception.Message;
            var response = ApiResponse<object>.Failure(message,statusCode);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );


        }
    }
}
