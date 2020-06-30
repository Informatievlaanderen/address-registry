namespace AddressRegistry.Api.Legacy.Tests.Framework.Generate
{
    using Legacy.AddressMatch.Requests;

    public static class AddressMatchRequestExtensions
    {
        public static AddressMatchRequest WithGemeenteAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
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

        public static AddressMatchRequest WithPostcodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
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

        public static AddressMatchRequest WithNisCodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
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

        public static AddressMatchRequest WithGemeenteAndKadStraatcode(this AddressMatchRequest request)
            => new AddressMatchRequest
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

        public static AddressMatchRequest WithPostcodeAndRrStraatcode(this AddressMatchRequest request)
            => new AddressMatchRequest
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

        public static AddressMatchRequest WithGemeenteAndNisCodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = "Springfield",
                Huisnummer = request.Huisnummer,
                Index = request.Index,
                KadStraatcode = request.KadStraatcode,
                Niscode = "12345",
                Postcode = request.Postcode,
                RrStraatcode = request.RrStraatcode,
                Straatnaam = "Evergreen Terrace"
            };
    }
}
