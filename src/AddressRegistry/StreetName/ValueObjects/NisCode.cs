namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class NisCode : StringValueObject<NisCode>
    {
        public NisCode([JsonProperty("value")] string nisCode) : base(nisCode)
        {
            if (string.IsNullOrWhiteSpace(nisCode))
                throw new ArgumentNullException(nameof(nisCode), "NisCode of a municipality cannot be empty.");
        }
    }
}
