namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Exceptions;

    public class BoxNumber : StringValueObject<BoxNumber>
    {
        private static readonly Regex FormatRegex
            = new ("^(?=.{1,10}$)-?(?:[A-Za-z0-9]|[A-Za-z0-9].*[A-Za-z0-9])$", RegexOptions.Compiled);

        internal BoxNumber(string boxNumber) : base(boxNumber.RemoveUnicodeControlCharacters()) { }

        public static BoxNumber Create(string boxNumber)
        {
            if (!HasValidFormat(boxNumber))
            {
                throw new BoxNumberHasInvalidFormatException();
            }

            return new BoxNumber(boxNumber);
        }

        public bool EqualsCaseSensitive(BoxNumber boxNumber)
        {
            return Value.Equals(boxNumber.Value);
        }

        public static bool HasValidFormat(string boxNumber, Regex? regex = null)
        {
            if (boxNumber == "0")
            {
                return false;
            }
            if (boxNumber.Contains("bus", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return regex?.IsMatch(boxNumber) ?? FormatRegex.IsMatch(boxNumber);
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}
