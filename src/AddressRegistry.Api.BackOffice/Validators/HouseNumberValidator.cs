namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;

    public static class HouseNumberValidator
    {
        private const string ValidationPattern = "^[1-9]([0-9]{0,8}([A-H]|[K-N]|[P]|[R-T]|[V-Z]){0,1}|[0-9]{0,9})$";
        private static readonly Regex FormatRegex = new Regex(ValidationPattern, RegexOptions.Compiled);

        public static bool IsValid(string houseNumber)
            => FormatRegex.IsMatch(houseNumber);
    }
}
