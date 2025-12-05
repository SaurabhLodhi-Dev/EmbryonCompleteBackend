using CleanArchitecture.WebApi.Middlewares;

namespace CleanArchitecture.WebApi.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseWrapperMiddleware>();
        }
    }
}
