namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class CorrectAddressPostalCodeRequestExamples : IExamplesProvider<CorrectAddressPostalCodeRequest>
    {
        public CorrectAddressPostalCodeRequest GetExamples()
        {
            return new CorrectAddressPostalCodeRequest
            {
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000"
            };
        }
    }
}
