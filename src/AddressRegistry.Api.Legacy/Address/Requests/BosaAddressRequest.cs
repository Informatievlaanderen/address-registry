namespace AddressRegistry.Api.Legacy.Address.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;

    public class BosaAddressRequest
    {
        public ZoekIdentifier AdresCode { get; set; }
        public string Huisnummer { get; set; }
        public string Busnummer { get; set; }
        public AdresStatus? AdresStatus { get; set; }
        public ZoekGeografischeNaam Straatnaam { get; set; }
        public ZoekIdentifier StraatnaamCode { get; set; }
        public ZoekGeografischeNaam Gemeentenaam { get; set; }
        public ZoekIdentifier GemeenteCode { get; set; }
        public ZoekIdentifier PostCode { get; set; }

        public bool IsOnlyAdresIdRequested
            => !string.IsNullOrEmpty(AdresCode?.ObjectId)
               && string.IsNullOrEmpty(Huisnummer)
               && string.IsNullOrEmpty(Busnummer)
               && (Straatnaam == null || (!Straatnaam.SearchType.HasValue && !Straatnaam.Taal.HasValue && string.IsNullOrEmpty(Straatnaam.Spelling)))
               && (StraatnaamCode == null || (!StraatnaamCode.VersieId.HasValue && string.IsNullOrEmpty(StraatnaamCode.ObjectId)))
               && (Gemeentenaam == null || (!Gemeentenaam.SearchType.HasValue && !Gemeentenaam.Taal.HasValue && string.IsNullOrEmpty(Gemeentenaam.Spelling)))
               && (GemeenteCode == null || (!GemeenteCode.VersieId.HasValue && string.IsNullOrEmpty(GemeenteCode.ObjectId)));
    }

    public class BosaAddressRequestExamples : IExamplesProvider<BosaAddressRequest>
    {
        public BosaAddressRequest GetExamples()
            => new BosaAddressRequest
            {
                AdresCode = new ZoekIdentifier
                {
                    ObjectId = "1",
                    VersieId = DateTimeOffset.Now.ToExampleOffset()
                },
                Huisnummer = "5",
                Busnummer = "001",
                Straatnaam = new ZoekGeografischeNaam
                {
                    Spelling = "school",
                    Taal = Taal.NL,
                    SearchType = BosaSearchType.Bevat
                },
                StraatnaamCode = new ZoekIdentifier
                {
                    ObjectId = "2",
                    VersieId = DateTimeOffset.Now.ToExampleOffset()
                },
                AdresStatus = AdresStatus.InGebruik,
                Gemeentenaam = new ZoekGeografischeNaam
                {
                    Spelling = "Brugge",
                    SearchType = BosaSearchType.Bevat,
                    Taal = Taal.NL,
                },
                GemeenteCode = new ZoekIdentifier
                {
                    ObjectId = "11001",
                    VersieId = DateTimeOffset.Now.ToExampleOffset()
                },
                PostCode = new ZoekIdentifier
                {
                    ObjectId = "9000",
                    VersieId = DateTimeOffset.Now.ToExampleOffset(),
                }
            };
    }
}
