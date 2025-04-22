using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OrderManagementSystem.API
{
    /// <summary>
    /// Middleware that catches ArgumentException thrown during request processing (including model binding)
    /// and returns a 400 Bad Request with a descriptive error message. This ensures that domain validation
    /// errors are translated into proper HTTP responses instead of unhandled exceptions.
    /// </summary>
    public class ArgumentExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ArgumentExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to catch ArgumentException and return a 400 Bad Request.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"{ex.Message.Replace("\"", "'" )}\"}}");
            }
        }
    }
}
