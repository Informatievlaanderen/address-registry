namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using AddressRegistry.Api.Oslo.AddressMatch.Requests;

    public static class AddressMatchRequestExtensions
    {
        public static AddressMatchRequest WithGemeenteAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = "Springfield",
                Huisnummer = request.Huisnummer,
                Niscode = request.Niscode,
                Postcode = request.Postcode,
                Straatnaam = "Evergreen Terrace"
            };

        public static AddressMatchRequest WithPostcodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = request.Gemeentenaam,
                Huisnummer = request.Huisnummer,
                Niscode = request.Niscode,
                Postcode = "49007",
                Straatnaam = "Evergreen Terrace"
            };

        public static AddressMatchRequest WithNisCodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = request.Gemeentenaam,
                Huisnummer = request.Huisnummer,
                Niscode = "12345",
                Postcode = request.Postcode,
                Straatnaam = "Evergreen Terrace"
            };

        public static AddressMatchRequest WithGemeenteAndKadStraatcode(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = "Springfield",
                Huisnummer = request.Huisnummer,
                Niscode = request.Niscode,
                Postcode = request.Postcode,
                Straatnaam = request.Straatnaam,
            };

        public static AddressMatchRequest WithPostcodeAndRrStraatcode(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = request.Gemeentenaam,
                Huisnummer = request.Huisnummer,
                Niscode = request.Niscode,
                Postcode = "9770",
                Straatnaam = request.Straatnaam
            };

        public static AddressMatchRequest WithGemeenteAndNisCodeAndStraatnaam(this AddressMatchRequest request)
            => new AddressMatchRequest
            {
                Busnummer = request.Busnummer,
                Gemeentenaam = "Springfield",
                Huisnummer = request.Huisnummer,
                Niscode = "12345",
                Postcode = request.Postcode,
                Straatnaam = "Evergreen Terrace"
            };
    }
}
