namespace AddressRegistry.Api.CrabImport.Infrastructure.Configuration
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Add headers to the response to prevent any caching.
    /// </summary>
    public class AddNoCacheHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public AddNoCacheHeadersMiddleware(RequestDelegate next) => _next = next;

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("cache-control", "no-store, no-cache, must-revalidate");
            context.Response.Headers.Add("pragma", "no-cache");

            return _next(context);
        }
    }
}
