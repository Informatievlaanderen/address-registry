namespace AddressRegistry
{
    using System.Linq;

    public static class StringExtensions
    {
        public static string RemoveUnicodeControlCharacters(this string input)
            => new string(input.Where(c => !char.IsControl(c)).ToArray());
    }
}
