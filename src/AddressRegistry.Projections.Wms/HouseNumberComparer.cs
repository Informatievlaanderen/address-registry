namespace AddressRegistry.Projections.Wms
{
    using System;
    using System.Collections.Generic;
    using AddressRegistry.StreetName;

    public class HouseNumberComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
                return 0;
            if (ReferenceEquals(x, null))
                return -1;
            if (ReferenceEquals(y, null))
                return 1;

            var digitsOfX = int.Parse(HouseNumber.HouseNumberDigits.Match(x).Value);
            var digitsOfY = int.Parse(HouseNumber.HouseNumberDigits.Match(y).Value);

            if (digitsOfX != digitsOfY) return digitsOfX.CompareTo(digitsOfY);

            var lettersOfX = HouseNumber.HouseNumberLetters.Match(x).Value;
            var lettersOfY = HouseNumber.HouseNumberLetters.Match(y).Value;

            return string.Compare(lettersOfX, lettersOfY, StringComparison.Ordinal);
        }
    }
}
