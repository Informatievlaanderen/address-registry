namespace AddressRegistry.Api.BackOffice.Infrastructure
{
    using System.Reflection;
    using Asp.Versioning;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Be.Vlaanderen.Basisregisters.Api;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Basisregisters Vlaanderen Address BackOffice Api {Assembly.GetEntryAssembly().GetVersionText()}.");
    }
}
