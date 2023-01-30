namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Swashbuckle.AspNetCore.Filters;

    public class CorrectAddressHouseNumberRequestExamples : IExamplesProvider<CorrectAddressHouseNumberRequest>
    {
        public CorrectAddressHouseNumberRequest GetExamples()
        {
            return new CorrectAddressHouseNumberRequest
            {
                Huisnummer = "11",
            };
        }
    }
}
