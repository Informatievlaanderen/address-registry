namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class Sanitizer
    {
        public class HouseNumberWithSubaddress
        {
            internal HouseNumberWithSubaddress(string houseNumber, string boxNumber, string appNumber)
            {
                HouseNumber = houseNumber;
                BoxNumber = boxNumber;
                AppNumber = appNumber;
            }

            public string HouseNumber { get; set; }
            public string BoxNumber { get; set; }
            public string AppNumber { get; set; }
        }

        private static Regex _containsNumber = new Regex("[0-9]+");
        //private static Regex brackets = new Regex("\\([A-Za-z0-9]*\\)");
        private static Regex _begin = new Regex("^[0-9]+|^[a-zA-Z]+");
        //private static HashSet<string> indexWordsBisnummer = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "bis", "ter" };
        private static readonly HashSet<string> _indexWordsBoxNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "BUS", "bte", "BT", "bu", "bs" };
        private static readonly HashSet<string> _indexWordsAppartementNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "AP", "APP", "apt" };
        private static readonly HashSet<string> _indexWordsFloorNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts" };
        private static HashSet<string> _indexWordsFloor = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL" };
        private static HashSet<string> _indexWordsWithoutBisNumber = new HashSet<string>(_indexWordsBoxNumber.Concat(_indexWordsAppartementNumber).Concat(_indexWordsFloorNumber).Concat(_indexWordsFloor), StringComparer.InvariantCultureIgnoreCase);
        public IWarningLogger Warnings { get; set; }

        public Sanitizer()
        {
            Warnings = new DefaultWarningLogger();
        }

        public List<HouseNumberWithSubaddress> Sanitize(string streetName, string houseNumber, string index)
        {
            //TODO: clean up Sammy code
            int hnr, bnr;

            //huisnummer formatteren
            var houseNumber0 = FormatRrHouseNumber(houseNumber);
            if (houseNumber0 == "00000000")
                Warnings.AddWarning("1", "Ongeldig 'Huisnummer'.");

            //rrindex formatteren
            var rrindex0 = FormatRrIndex(index);

            /* huisnummer in straatnaam */
            if ((string.IsNullOrEmpty(houseNumber) || houseNumber0 == "00000000") && string.IsNullOrEmpty(index) && !string.IsNullOrEmpty(streetName))
            {
                var requestStraatnaamWithoutStraatnaam = streetName.Replace(StripStreetName(streetName), "").Replace(",", string.Empty).Replace(" ", string.Empty);
                //huisnummer als prefix in straatnaam
                if (new Regex("[0-9] ").IsMatch(streetName))
                {
                    if (FormatRrIndex(RightIndex(requestStraatnaamWithoutStraatnaam)) == "0000"
                        && FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)) != null
                        && Int32.TryParse(FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)), out hnr)
                        && hnr != 0)
                    {
                        houseNumber0 = FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam));
                    }
                }
                //huisnummer met niet-numeriek bisnummer als prefix in straatnaam
                if (new Regex("[0-9][a-zA-Z] ").IsMatch(streetName) && !new Regex("[0-9]e ").IsMatch(streetName))
                {
                    if (FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)) != null
                        && Int32.TryParse(FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)), out hnr)
                        && hnr != 0
                        && new Regex("[0-9][a-zA-Z]$").IsMatch(requestStraatnaamWithoutStraatnaam))
                    {
                        houseNumber0 = FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam));
                        rrindex0 = FormatRrIndex(RightIndex(requestStraatnaamWithoutStraatnaam)).ToUpper();
                    }
                }
                //huisnummer als suffix in straatnaam
                if (new Regex(" [0-9]+$").IsMatch(streetName))
                {
                    houseNumber0 = FormatRrHouseNumber(new Regex(" [0-9]+$").Match(streetName).Value);
                    //TODO Stif: ask business, why only here warning?
                    Warnings.AddWarning("2", "'Huisnummer' in 'Straatnaam' gevonden.");
                }
                //huisnummer met niet-numeriek bisnummer als suffix in straatnaam
                if (new Regex(" [0-9]+[a-zA-Z]$").IsMatch(streetName))
                {
                    if (FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)) != null
                        && Int32.TryParse(FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam)), out hnr)
                        && hnr != 0
                        && new Regex("[0-9][a-zA-Z]$").IsMatch(requestStraatnaamWithoutStraatnaam))
                    {
                        houseNumber0 = FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam));
                        rrindex0 = FormatRrIndex(RightIndex(requestStraatnaamWithoutStraatnaam)).ToUpper();
                        //TODO Stif: ask business, why only here warning?
                        Warnings.AddWarning("3", "Niet-numeriek 'Huisnummer' in 'Straatnaam' gevonden.");
                    }
                }
            }

            /* huisnummer zonder RR-index */
            /* Het huisnummer wordt een CRAB-huisnummer. Er is geen CRAB-subadres. */
            /********************************************************/
            //volledig numerisch
            if (new Regex("^[0-9]+$").IsMatch(houseNumber0) && rrindex0 == "0000")
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, null) };
            }
            //niet-numeriek bisnummer: enkele letter
            if (rrindex0 == "0000" && new Regex(@"^[0-9]+\s*[a-zA-Z]$").IsMatch(houseNumber0))
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0.Substring(0, houseNumber0.Length - 1).Trim()) + houseNumber0.Last().ToString().ToUpper(), null, null) };
            }
            //niet-numeriek bisnummer: 'bis'
            if (rrindex0 == "0000" && houseNumber0.EndsWith("bis", StringComparison.InvariantCultureIgnoreCase) && !new Regex("[a-zA-Z]").IsMatch(houseNumber0.Substring(0, houseNumber0.Length - 3)))
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0.Substring(0, houseNumber0.Length - 3)) + "BIS", null, null) };
            }

            /* huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en = 0 */
            /* Het huisnummer + deel1 wordt een CRAB-huisnummer met een niet-numeriek bisnummer. Er is geen CRAB-subadres.
               Als deel1 een aanduiding is van een subadres wordt enkel het RR-huisnummer als huisnummer weggeschreven.
               Als deel1 een aanduiding is van een verdiep wordt enkel het RR-huisnummer als huisnummer weggeschreven en een subadres 0.0 met aard appartementnummer. */
            /**********************************************************************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && new Regex("^[a-zA-Z]").IsMatch(rrindex0) && (RightIndex(rrindex0) == string.Empty || RemovePrecedingZeros(RightIndex(rrindex0)) == string.Empty))
            {
                if (!_indexWordsWithoutBisNumber.Contains(LeftIndex(rrindex0)))
                {
                    //niet-numeriek bisnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + LeftIndex(rrindex0), null, null) };
                }
                else
                {
                    //geen niet-numeriek bisnummer
                    if (!_indexWordsFloorNumber.Contains(LeftIndex(rrindex0)))
                    {
                        //appartementnummer 0
                        if (_indexWordsFloor.Contains(LeftIndex(rrindex0)))
                            return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, "0.0") };

                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, null) };
                    }
                    else
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, null) };
                    }
                }
            }

            /* huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en > 0 */
            /* Het huisnummer wordt een CRAB-huisnummer, deel2 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
               Als deel1 een aanduiding is van een subadres wordt enkel het huisnummer als huisnummer weggeschreven. */
            /*********************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && new Regex("^[a-zA-Z]").IsMatch(rrindex0) && Int32.TryParse(RightIndex(rrindex0), out bnr) && bnr > 0)
            {
                if (!_indexWordsWithoutBisNumber.Contains(LeftIndex(rrindex0)))
                {
                    //niet-numeriek bisnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + LeftIndex(rrindex0), RemovePrecedingZeros(RightIndex(rrindex0)), null) };
                }
                else
                {
                    //geen niet-numeriek bisnummer
                    if (_indexWordsBoxNumber.Contains(LeftIndex(rrindex0)))
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), RemovePrecedingZeros(RightIndex(rrindex0)), null) };
                    }
                    else
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, LeftIndex(rrindex0) + RemovePrecedingZeros(RightIndex(rrindex0))) };
                    }
                }
            }

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en zonder deel 3. */
            /* Het huisnummer wordt een CRAB-huisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.*/
            /**************************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && !string.IsNullOrEmpty(rrindex0) && rrindex0 != "0000" && new Regex("^[0-9]").IsMatch(rrindex0) && LeftIndex(RightIndex(rrindex0)) == string.Empty)
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), RemovePrecedingZeros(LeftIndex(rrindex0)), null) };
            }

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en met numeriek deel 3 en zonder deel 4 */
            /* Het huisnummer wordt een CRAB-huisnummer, de RR-index wordt een CRAB-subadres van type appartementnummer. */
            /*************************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && !string.IsNullOrEmpty(rrindex0) && rrindex0 != "0000" && new Regex("^[0-9]").IsMatch(rrindex0) && Int32.TryParse(LeftIndex(RightIndex(rrindex0)), out bnr) && RightIndex(RightIndex(rrindex0)) == string.Empty)
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, index) };
            }

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en numeriek deel 4 */
            /* Het huisnummer + deel1 wordt een CRAB-huisnummer met een numeriek bisnummer, deel4 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
               Als deel3 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 en deel4 samengevoegd tot een appartementnummer.*/
            /***************************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && !string.IsNullOrEmpty(rrindex0) && rrindex0 != "0000" && new Regex("^[a-zA-Z]").IsMatch(LeftIndex(RightIndex(rrindex0)))
                && Int32.TryParse(RightIndex(RightIndex(rrindex0)), out bnr))
            {
                if (!_indexWordsWithoutBisNumber.Contains(LeftIndex(RightIndex(rrindex0))))
                {
                    // niet-numeriek bisnummer
                    var bus = RemovePrecedingZeros(RightIndex(RightIndex(rrindex0)));
                    bus = bus == string.Empty ? "0" : bus;
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + LeftIndex(RightIndex(rrindex0)), bus, null) };
                }
                else if (_indexWordsFloorNumber.Contains(LeftIndex(RightIndex(rrindex0))))
                {
                    // verdiepnummer
                    var part1 = RemovePrecedingZeros(LeftIndex(rrindex0));
                    part1 = part1 == string.Empty ? "0" : part1;
                    var part2 = RemovePrecedingZeros(RightIndex(RightIndex(rrindex0)));
                    part2 = part2 == string.Empty ? "0" : part2;
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, part1 + "." + part2) };
                }
                else if (_indexWordsAppartementNumber.Contains(LeftIndex(RightIndex(rrindex0))))
                {
                    // appartementnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + RemovePrecedingZeros(LeftIndex(rrindex0)), null, RemovePrecedingZeros(RightIndex(RightIndex(rrindex0)))) };
                }
                else if (_indexWordsBoxNumber.Contains(LeftIndex(RightIndex(rrindex0))))
                {
                    // busnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + RemovePrecedingZeros(LeftIndex(rrindex0)), RemovePrecedingZeros(RightIndex(RightIndex(rrindex0))), null) };
                }
            }

            /* RR-huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en zonder deel 4
	           Het RR-huisnummer+deel1 wordt een CRAB-huisnummer met numeriek bisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
	           Als deel2 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 het verdiepnummer van appartementnummer 0.*/
            /*****************************************************************************************************************/
            if (Int32.TryParse(houseNumber0, out hnr) && !string.IsNullOrEmpty(rrindex0) && rrindex0 != "0000" && new Regex("^[0-9]").IsMatch(rrindex0)
                && new Regex("^[a-zA-Z]").IsMatch(LeftIndex(RightIndex(rrindex0))) && RightIndex(RightIndex(rrindex0)) == string.Empty)
            {
                if (_indexWordsFloorNumber.Contains(RightIndex(rrindex0)))
                {
                    // verdiepnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0), null, RemovePrecedingZeros(LeftIndex(rrindex0)) + ".0") };
                }
                else
                {
                    //geen verdiepnummer
                    return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(RemovePrecedingZeros(houseNumber0) + "_" + RemovePrecedingZeros(LeftIndex(rrindex0)), RightIndex(rrindex0), null) };
                }
            }

            /* bereiken                        */
            /***********************************/
            // detecteer bereikachtige records
            string houseNumber1 = null;
            string houseNumber2 = null;
            if (rrindex0 == "0000" || _indexWordsWithoutBisNumber.Contains(rrindex0))
            {
                if (houseNumber != null && houseNumber.Contains("-") && Int32.TryParse(houseNumber.Split('-')[0].Trim(), out hnr))
                {
                    houseNumber1 = houseNumber.Split('-')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('-') + 1).Trim();
                }
                if (houseNumber != null && houseNumber.Contains("/") && Int32.TryParse(houseNumber.Split('/')[0].Trim(), out hnr))
                {
                    houseNumber1 = houseNumber.Split('/')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('/') + 1).Trim();
                }
                if (houseNumber != null && houseNumber.Contains("+") && Int32.TryParse(houseNumber.Split('+')[0].Trim(), out hnr))
                {
                    houseNumber1 = houseNumber.Split('+')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('+') + 1).Trim();
                }
                if (houseNumber != null && new Regex("[0-9] [0-9]").IsMatch(houseNumber) && Int32.TryParse(houseNumber.Split(' ')[0].Trim(), out hnr))
                {
                    houseNumber1 = houseNumber.Split(' ')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf(' ') + 1).Trim();
                }
                if (streetName != null && new Regex("[0-9]+-[0-9]+").IsMatch(streetName))
                {
                    var range = new Regex("[0-9]+-[0-9]+").Match(streetName);
                    houseNumber1 = range.Value.Split('-')[0];
                    houseNumber2 = range.Value.Substring(range.Value.IndexOf('-') + 1);
                }
            }
            //omzetten van bereik naar adres
            if (houseNumber1 != null)
            {
                if (!Int32.TryParse(houseNumber2, out hnr))
                {
                    // niet-numeriek bisnummer genoteerd als bereik
                    if (new Regex("[a-zA-Z]").IsMatch(houseNumber2))
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(houseNumber1, houseNumber2.ToUpper(), null) };
                    }
                    // enkel huisnummer genoteerd als bereik
                    else
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(houseNumber1, null, null) };
                    }
                }
                else
                {
                    // subadres genoteerd als bereik
                    if (Int32.Parse(houseNumber1) > hnr)
                    {
                        return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(houseNumber1, houseNumber2, null) };
                    }
                    else
                    {
                        return new List<HouseNumberWithSubaddress> {
                            new HouseNumberWithSubaddress(houseNumber1, null, null),
                            new HouseNumberWithSubaddress(houseNumber2, null, null)
                        };
                    }
                }
            }
            //last fallback
            if (rrindex0 == "0000" && new Regex("[0-9]+").IsMatch(RemovePrecedingZeros(houseNumber0)))
            {
                return new List<HouseNumberWithSubaddress> { new HouseNumberWithSubaddress(new Regex("[0-9]+").Match(RemovePrecedingZeros(houseNumber0)).Value, null, null) };
            }

            return new List<HouseNumberWithSubaddress>();
        }

        private static string FormatRrHouseNumber(string rrHouseNumber)
        {
            if (string.IsNullOrEmpty(rrHouseNumber))
                return string.Empty;

            return rrHouseNumber.Trim().PadLeft(8, '0');
        }

        private static string FormatRrIndex(string rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
                return "0000";

            var formatted = RemovePrecedingZeros(rrIndex).Trim();

            if (_containsNumber.IsMatch(formatted))
                formatted = formatted.PadLeft(4, '0');
            else
                formatted = formatted.PadRight(4, '0');

            return formatted;
        }

        public static string StripStreetName(string streetName)
        {
            //TODO: optimize

            String trimmed = streetName;

            // straatnaam, hnr [busnr]
            if (trimmed.Contains(","))
            {
                trimmed = trimmed.Substring(0, trimmed.IndexOf(","));
            }

            Match m = _containsNumber.Match(trimmed);
            if (m.Length > 0)
            {
                // hrn[busnr] straatnaam
                if (m.Index == 0)
                {
                    trimmed = trimmed.Replace(m.Value, "");
                    while (!trimmed.StartsWith(" ") && trimmed.Length > 0)
                        trimmed = trimmed.Substring(1, trimmed.Length - 1);
                    trimmed = trimmed.Trim();
                }

                // straatnaam hnr [busnr]
                if (m.Index > 0)
                {
                    trimmed = trimmed.Substring(0, trimmed.Length - (trimmed.Length - m.Index));
                    trimmed = trimmed.Trim();
                }
            }

            return trimmed;
        }

        private static string RightIndex(string rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
                return string.Empty;

            var rightPart = string.Empty;

            if (_begin.IsMatch(rrIndex))
                rightPart = rrIndex.Substring(_begin.Match(rrIndex).Value.Length);
            while (!_begin.IsMatch(rightPart) && rightPart.Length > 0)
                rightPart = rightPart.Substring(1);

            return rightPart;
        }

        private static string LeftIndex(string rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
                return string.Empty;

            var leftPart = string.Empty;
            if (_begin.IsMatch(rrIndex))
                leftPart = _begin.Match(rrIndex).Value;

            return leftPart;
        }

        private static string RemovePrecedingZeros(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.TrimStart(new char[] { '0' });
        }
    }
}
