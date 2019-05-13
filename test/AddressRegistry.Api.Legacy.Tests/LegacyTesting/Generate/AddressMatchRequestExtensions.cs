namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using AddressMatch.Requests;

    public static class AddressMatchRequestExtensions
    {
        public static AddressMatchRequest WithGemeenteAndStraatnaam(this AddressMatchRequest request)
        {
            return
                new AddressMatchRequest
                {
                    Busnummer = request.Busnummer,
                    Gemeentenaam = "Springfield",
                    Huisnummer = request.Huisnummer,
                    Index = request.Index,
                    KadStraatcode = request.KadStraatcode,
                    Niscode = request.Niscode,
                    Postcode = request.Postcode,
                    RrStraatcode = request.RrStraatcode,
                    Straatnaam = "Evergreen Terrace"
                };
        }

        public static AddressMatchRequest WithPostcodeAndStraatnaam(this AddressMatchRequest request)
        {
            return
                new AddressMatchRequest
                {
                    Busnummer = request.Busnummer,
                    Gemeentenaam = request.Gemeentenaam,
                    Huisnummer = request.Huisnummer,
                    Index = request.Index,
                    KadStraatcode = request.KadStraatcode,
                    Niscode = request.Niscode,
                    Postcode = "49007",
                    RrStraatcode = request.RrStraatcode,
                    Straatnaam = "Evergreen Terrace"
                };
        }

        public static AddressMatchRequest WithNISCodeAndStraatnaam(this AddressMatchRequest request)
        {
            return
                new AddressMatchRequest
                {
                    Busnummer = request.Busnummer,
                    Gemeentenaam = request.Gemeentenaam,
                    Huisnummer = request.Huisnummer,
                    Index = request.Index,
                    KadStraatcode = request.KadStraatcode,
                    Niscode = "12345",
                    Postcode = request.Postcode,
                    RrStraatcode = request.RrStraatcode,
                    Straatnaam = "Evergreen Terrace"
                };
        }

        public static AddressMatchRequest WithGemeenteAndKadStraatcode(this AddressMatchRequest request)
        {
            return
                new AddressMatchRequest
                {
                    Busnummer = request.Busnummer,
                    Gemeentenaam = "Springfield",
                    Huisnummer = request.Huisnummer,
                    Index = request.Index,
                    KadStraatcode = "6789",
                    Niscode = request.Niscode,
                    Postcode = request.Postcode,
                    RrStraatcode = request.RrStraatcode,
                    Straatnaam = request.Straatnaam,
                };
        }

        public static AddressMatchRequest WithPostcodeAndRrStraatcode(this AddressMatchRequest request)
        {
            return
                new AddressMatchRequest
                {
                    Busnummer = request.Busnummer,
                    Gemeentenaam = request.Gemeentenaam,
                    Huisnummer = request.Huisnummer,
                    Index = request.Index,
                    KadStraatcode = request.KadStraatcode,
                    Niscode = request.Niscode,
                    Postcode = "9770",
                    RrStraatcode = "987",
                    Straatnaam = request.Straatnaam
                };
        }
    }
}
