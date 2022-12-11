namespace AddressRegistry.Api.Legacy.Address.BosaRepresentation
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using MediatR;
    using Swashbuckle.AspNetCore.Filters;

    public class AddressRepresentationBosaRequest : IRequest<AddressRepresentationBosaResponse>
    {
        public ZoekIdentifier AdresCode { get; set; }
        public Taal? Taal { get; set; }
    }

    public class AddressRepresentationBosaRequestExamples : IExamplesProvider<AddressRepresentationBosaRequest>
    {
        public AddressRepresentationBosaRequest GetExamples()
            => new AddressRepresentationBosaRequest
            {
                AdresCode = new ZoekIdentifier
                {
                    ObjectId = "1",
                    VersieId = DateTimeOffset.Now.ToExampleOffset()
                },
                Taal = Taal.NL
            };
    }
}
