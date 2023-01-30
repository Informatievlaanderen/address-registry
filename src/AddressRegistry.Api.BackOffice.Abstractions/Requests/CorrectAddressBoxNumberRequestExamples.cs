namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class CorrectAddressBoxNumberRequestExamples : IExamplesProvider<CorrectAddressBoxNumberRequest>
    {
        public CorrectAddressBoxNumberRequest GetExamples()
        {
            return new CorrectAddressBoxNumberRequest
            {
                Busnummer = "1A",
            };
        }
    }
}
