namespace AddressRegistry.StreetName
{
    using System;
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Exceptions;

    public class BoxNumber : StringValueObject<BoxNumber>
    {
        public BoxNumber(string boxNumber) : base(boxNumber.RemoveUnicodeControlCharacters()) { }

        private static readonly Regex FormatRegex = new ("^[a-zA-Z0-9]{1,10}$", RegexOptions.Compiled);

        public static BoxNumber Create(string boxNumber)
        {
            if (!HasValidFormat(boxNumber))
            {
                throw new BoxNumberHasInvalidFormatException();
            }

            return new BoxNumber(boxNumber);
        }

        public static bool HasValidFormat(string? boxNumber)
        {
            if (boxNumber is null)
            {
                return true;
            }
            if (boxNumber == "0")
            {
                return false;
            }
            if (boxNumber.Contains("bus", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return FormatRegex.IsMatch(boxNumber);
        }
    }
}
