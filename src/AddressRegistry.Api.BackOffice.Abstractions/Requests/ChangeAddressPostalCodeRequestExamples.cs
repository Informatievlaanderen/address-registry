namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class ChangeAddressPostalCodeRequestExamples : IExamplesProvider<ChangeAddressPostalCodeRequest>
    {
        public ChangeAddressPostalCodeRequest GetExamples()
        {
            return new ChangeAddressPostalCodeRequest
            {
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000"
            };
        }
    }
}
