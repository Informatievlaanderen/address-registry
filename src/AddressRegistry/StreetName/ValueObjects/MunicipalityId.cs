namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class MunicipalityId : GuidValueObject<MunicipalityId>
    {
        public static MunicipalityId CreateFor(string municipalityId)
            => new MunicipalityId(Guid.Parse(municipalityId));

        public MunicipalityId([JsonProperty("value")] Guid municipalityId) : base(municipalityId) { }
    }
}
