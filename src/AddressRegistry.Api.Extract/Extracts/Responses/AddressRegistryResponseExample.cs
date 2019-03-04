namespace AddressRegistry.Api.Extract.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Filters;
    using System;

    public class AddressRegistryResponseExample : IExamplesProvider
    {
        public object GetExamples()
            => new { mimeType = "application/zip", fileName = $"{ExtractController.ZipName}-{DateTime.Now:yyyy-MM-dd}.zip" };
    }
}
