using CleanArchitecture.WebApi.Middlewares;

namespace CleanArchitecture.WebApi.Extensions
{
    public static class GeoLocationExtensions
    {
        public static IApplicationBuilder UseGeoLocation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GeoLocationMiddleware>();
        }
    }
}
