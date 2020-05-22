namespace AddressRegistry.Api.Extract.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Filters;

    public class AddressRegistryResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
            => new { mimeType = "application/zip", fileName = $"{ExtractFileNames.GetAddressZip()}.zip" };
    }
}
