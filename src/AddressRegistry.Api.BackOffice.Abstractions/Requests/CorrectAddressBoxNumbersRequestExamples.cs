namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class CorrectAddressBoxNumbersRequestExamples : IExamplesProvider<CorrectAddressBoxNumbersRequest>
    {
        public CorrectAddressBoxNumbersRequest GetExamples()
        {
            return new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [new CorrectAddressBoxNumbersRequestItem
                {
                    AdresId = "https://data.vlaanderen.be/id/adres/200001",
                    Busnummer = "1A"
                }],
            };
        }
    }
}
