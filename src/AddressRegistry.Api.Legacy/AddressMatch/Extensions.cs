namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public static class Extensions
    {
        public static bool In<T>(this T element, IEnumerable<T> elements)
        {
            return elements.Any(t => EqualityComparer<T>.Default.Equals(t, element));
        }

        public static bool EqIgnoreCase(this string a, string b)
        {
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqIgnoreDiacritics(this string a, string b)
        {
            return a.RemoveDiacritics().Equals(b.RemoveDiacritics());
        }

        public static bool EqIgnoreCaseAndDiacritics(this string a, string b)
        {
            return a.RemoveDiacritics().EqIgnoreCase(b.RemoveDiacritics());
        }

        public static bool EqFuzzyMatch(this string a, string b, double threshold)
        {
            return a.FuzzyScore(b) > threshold;
        }

        public static bool EqFuzzyMatchToggleAbreviations(this string a, string b, double threshold)
        {
            return a.EqFuzzyMatch(b.ToggleAbbreviations(), threshold);
        }

        public static bool ContainsIgnoreCase(this string a, string b)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(a, b, CompareOptions.IgnoreCase) >= 0;
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static double FuzzyScore(this string a, string b)
        {
            return FuzzyMatch.Calculate(a, b);
        }

        public static string ToggleAbbreviations(this string input)
        {
            input = input.ToLowerInvariant();

            if (input.Contains("onze lieve vrouw"))
                return input.Replace("onze lieve vrouw", "O.L. Vrouw");
            if (input.Contains("o.l.v."))
                return input.Replace("o.l.v.", "Onze Lieve Vrouw");
            if (input.Contains("sint"))
                return input.Replace("sint", "st.");
            if (input.Contains("st."))
                return input.Replace("st.", "sint");
            if (input.Contains("dr."))
                return input.Replace("dr.", "dokter");
            if (input.Contains("dokter"))
                return input.Replace("dokter", "dr.");
            if (input.Contains("stwg."))
                return input.Replace("stwg.", "steenweg");
            if (input.Contains("stwg"))
                return input.Replace("stwg", "steenweg");
            if (input.Contains("stw."))
                return input.Replace("stw.", "steenweg");
            if (input.Contains("stw"))
                return input.Replace("stw", "steenweg");
            if (input.Contains("burg."))
                return input.Replace("burg.", "Burgemeester");
            if (input.Contains("burgemeester"))
                return input.Replace("Burgemeester", "burg.");
            if (input.EndsWith("str"))
                return input.Replace("str", "straat");
            if (input.EndsWith("str."))
                return input.Replace("str.", "straat");

            return input;
        }
    }
}
