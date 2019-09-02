namespace AddressRegistry.Api.Extract.Infrastructure
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.Api;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ControllerBase
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Basisregisters Vlaanderen Address Extract Api {Assembly.GetEntryAssembly().GetVersionText()}.");
    }
}
