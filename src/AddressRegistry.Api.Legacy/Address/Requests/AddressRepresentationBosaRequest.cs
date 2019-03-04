namespace AddressRegistry.Api.Legacy.Address.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using Swashbuckle.AspNetCore.Filters;
    using System;

    public class AddressRepresentationBosaRequest
    {
        public ZoekIdentifier AdresCode { get; set; }
        public Taal? Taal { get; set; }
    }

    public class AddressRepresentationBosaRequestExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new AddressRepresentationBosaRequest
            {
                AdresCode = new ZoekIdentifier
                {
                    ObjectId = "1",
                    VersieId = DateTimeOffset.Now
                },
                Taal = Taal.NL
            };
        }
    }
}
