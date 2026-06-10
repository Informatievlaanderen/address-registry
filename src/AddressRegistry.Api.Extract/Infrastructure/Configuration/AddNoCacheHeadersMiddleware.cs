namespace AddressRegistry.Api.Extract.Infrastructure.Configuration
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Add headers to the response to prevent any caching.
    /// </summary>
    public class AddNoCacheHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public AddNoCacheHeadersMiddleware(RequestDelegate next) => _next = next;

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Append(HeaderNames.CacheControl, "no-store, no-cache, must-revalidate");
            context.Response.Headers.Append(HeaderNames.Pragma, "no-cache");
            context.Response.Headers.Append(HeaderNames.Expires, "0");

            return _next(context);
        }
    }
}
