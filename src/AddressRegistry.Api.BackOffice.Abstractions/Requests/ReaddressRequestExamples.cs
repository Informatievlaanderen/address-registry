namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Swashbuckle.AspNetCore.Filters;

    public class ReaddressRequestExamples : IExamplesProvider<ReaddressRequest>
    {
        public ReaddressRequest GetExamples()
        {
            return new ReaddressRequest
            {
                DoelStraatnaamId = "https://data.vlaanderen.be/id/straatnaam/45041",
                HerAdresseer = new List<AddressToReaddressItem>
                {
                    new AddressToReaddressItem
                    {
                        BronAdresId = "https://data.vlaanderen.be/id/adres/200001",
                        DoelHuisnummer = "25"
                    },
                    new AddressToReaddressItem
                    {
                        BronAdresId = "https://data.vlaanderen.be/id/adres/200002",
                        DoelHuisnummer = "27"
                    }
                },
                OpheffenAdressen = new List<string>
                {
                    "https://data.vlaanderen.be/id/adres/200001"
                }
            };
        }
    }
}
