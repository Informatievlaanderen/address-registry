namespace AddressRegistry.Api.BackOffice.Validators
{
    using System;
    using System.Text.RegularExpressions;

    public static class BoxNumberValidator
    {
        private const string ValidationPattern = "^[a-zA-Z0-9]{1,10}$";
        private static readonly Regex FormatRegex = new Regex(ValidationPattern, RegexOptions.Compiled);

        public static bool IsValid(string? boxNumber)
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
