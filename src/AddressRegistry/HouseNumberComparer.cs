namespace AddressRegistry
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public sealed class HouseNumberComparer : IComparer<string>

    {
        private static readonly Regex HouseNumberDigits =
            new (@"\d+", RegexOptions.Compiled);

        private static readonly Regex HouseNumberLetters =
            new (@"([A-Z]){0,1}$", RegexOptions.Compiled);

        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
                return 0;
            if (ReferenceEquals(x, null))
                return -1;
            if (ReferenceEquals(y, null))
                return 1;

            var possibleDigitsOfX = HouseNumberDigits.Match(x).Value;
            var possibleDigitsOfY = HouseNumberDigits.Match(y).Value;

            var xHasDigits = !string.IsNullOrWhiteSpace(possibleDigitsOfX);
            var yHasDigits = !string.IsNullOrWhiteSpace(possibleDigitsOfY);

            if (xHasDigits && yHasDigits)
            {
                var digitsOfX = int.Parse(possibleDigitsOfX);
                var digitsOfY = int.Parse(possibleDigitsOfY);

                if (digitsOfX != digitsOfY) return digitsOfX.CompareTo(digitsOfY);
            }

            if (xHasDigits && !yHasDigits)
                return -1;

            if (yHasDigits && !xHasDigits)
                return 1;

            var lettersOfX = HouseNumberLetters.Match(x).Value;
            var lettersOfY = HouseNumberLetters.Match(y).Value;

            return string.Compare(lettersOfX, lettersOfY, StringComparison.Ordinal);
        }
    }
}
