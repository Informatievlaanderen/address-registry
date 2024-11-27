namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Exceptions;

    public class HouseNumber : StringValueObject<HouseNumber>
    {
        private static readonly Regex FormatRegex = new("^[1-9]([0-9]{0,8}([A-Z]){0,1}|[0-9]{0,9})$", RegexOptions.Compiled);

        internal HouseNumber(string houseNumber) : base(houseNumber.RemoveUnicodeControlCharacters()) { }

        public static HouseNumber Create(string houseNumber)
        {
            if (!HasValidFormat(houseNumber))
            {
                throw new HouseNumberHasInvalidFormatException(houseNumber);
            }

            return new HouseNumber(houseNumber);
        }

        public bool EqualsCaseSensitive(HouseNumber houseNumber)
        {
            return Value.Equals(houseNumber.Value);
        }

        public static bool HasValidFormat(string houseNumber) => FormatRegex.IsMatch(houseNumber);

        protected override IEnumerable<object> Reflect()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}
