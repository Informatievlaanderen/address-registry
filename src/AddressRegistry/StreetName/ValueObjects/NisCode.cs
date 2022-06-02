namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class NisCode : StringValueObject<NisCode>
    {
        public NisCode(string nisCode) : base(nisCode)
        {
            if (string.IsNullOrWhiteSpace(nisCode))
                throw new ArgumentNullException(nameof(nisCode), "NisCode of a municipality cannot be empty.");
        }
    }
}
