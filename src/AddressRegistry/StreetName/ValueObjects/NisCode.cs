namespace AddressRegistry.StreetName
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class NisCode : StringValueObject<NisCode>
    {
        public NisCode(string nisCode) : base(nisCode)
        {
            if (string.IsNullOrWhiteSpace(nisCode))
                throw new ArgumentNullException(nameof(nisCode), "NisCode of a municipality cannot be empty.");
        }

        public static bool IsValid(string nisCode) => nisCode.Length == 5 && nisCode.All(char.IsDigit);
    }
}
