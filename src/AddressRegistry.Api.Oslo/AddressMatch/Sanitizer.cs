namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public sealed class Sanitizer
    {
        private static readonly Regex ContainsNumber = new Regex("[0-9]+");
        private static readonly Regex Begin = new Regex("^[0-9]+|^[a-zA-Z]+");

        private static readonly HashSet<string> IndexWordsBoxNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "BUS", "bte", "BT", "bu", "bs" };
        private static readonly HashSet<string> IndexWordsAppartementNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "AP", "APP", "apt" };
        private static readonly HashSet<string> IndexWordsFloorNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts" };
        private static readonly HashSet<string> IndexWordsFloor = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL" };
        private static readonly HashSet<string> IndexWordsWithoutBisNumber = new HashSet<string>(IndexWordsBoxNumber.Concat(IndexWordsAppartementNumber).Concat(IndexWordsFloorNumber).Concat(IndexWordsFloor), StringComparer.InvariantCultureIgnoreCase);

        public IWarningLogger Warnings { get; set; }

        public Sanitizer()
            => Warnings = new DefaultWarningLogger();

        public List<HouseNumberWithSubaddress> Sanitize(
            string? streetName,
            string? houseNumber,
            string? index,
            bool strictMatch = false)
        {
            // huisnummer formatteren
            var formattedHouseNumber = FormatRrHouseNumber(houseNumber);
            if (formattedHouseNumber == "00000000")
            {
                Warnings.AddWarning("1", "Ongeldig 'Huisnummer'.");
            }

            // rrindex formatteren
            var formattedRrIndex = FormatRrIndex(index);

            // huisnummer in straatnaam
            if ((string.IsNullOrEmpty(houseNumber) || formattedHouseNumber == "00000000") &&
                string.IsNullOrEmpty(index) &&
                !string.IsNullOrEmpty(streetName))
            {
                var formattedParts = ParseHouseNumberFromStreetName(streetName, formattedHouseNumber, formattedRrIndex);

                formattedHouseNumber = FormatRrHouseNumber(formattedParts.HouseNumber);
                formattedRrIndex = FormatRrIndex(formattedParts.RrIndex);
            }

            // huisnummer zonder RR-index
            // Het huisnummer wordt een CRAB-huisnummer. Er is geen CRAB-subadres.
            // *******************************************************************
            if (HouseNumberWithoutIndex(formattedRrIndex))
            {
                var potentialMatch = MatchHouseNumberWithoutIndex(formattedHouseNumber, strictMatch);

                if (potentialMatch.Any())
                {
                    return potentialMatch;
                }
            }

            // huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en gelijk aan 0
            // Het huisnummer + deel1 wordt een CRAB-huisnummer met een niet-numeriek bisnummer. Er is geen CRAB-subadres.
            // Als deel1 een aanduiding is van een subadres wordt enkel het RR-huisnummer als huisnummer weggeschreven.
            // Als deel1 een aanduiding is van een verdiep wordt enkel het RR-huisnummer als huisnummer weggeschreven en een subadres 0.0 met aard appartementnummer.
            // ******************************************************************************************************************************************************
            if (HouseNumberWithIndex_Part1NonNumeric_Part2Zero(formattedHouseNumber, formattedRrIndex))
            {
                return MatchHouseNumberWithIndex_Part1NonNumeric_Part2Zero(formattedHouseNumber, formattedRrIndex);
            }

            // huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en > 0
            // Het huisnummer wordt een CRAB-huisnummer, deel2 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
            // Als deel1 een aanduiding is van een subadres wordt enkel het huisnummer als huisnummer weggeschreven.
            // *****************************************************************************************************
            if (HouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero(formattedHouseNumber, formattedRrIndex))
            {
                return MatchHouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero(formattedHouseNumber, formattedRrIndex);
            }

            // huisnummers met RR-index waarvan deel1 begint met een cijfer en zonder deel 3.
            // Het huisnummer wordt een CRAB-huisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
            // ***********************************************************************************************************
            if (HouseNumberWithIndex_Part1StartNumber_Part3Missing(formattedHouseNumber, formattedRrIndex))
            {
                return MatchHouseNumberWithIndex_Part1StartNumber_Part3Missing(formattedHouseNumber, formattedRrIndex);
            }

            // huisnummers met RR-index waarvan deel1 begint met een cijfer en met numeriek deel 3 en zonder deel 4
            // Het huisnummer wordt een CRAB-huisnummer, de RR-index wordt een CRAB-subadres van type appartementnummer.
            // *********************************************************************************************************
            if (HouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing(formattedHouseNumber, formattedRrIndex))
            {
                return MatchHouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing(formattedHouseNumber, formattedRrIndex);
            }

            // huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en numeriek deel 4
            // Het huisnummer + deel1 wordt een CRAB-huisnummer met een numeriek bisnummer, deel4 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
            // Als deel3 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 en deel4 samengevoegd tot een appartementnummer.
            // *****************************************************************************************************************************************************************************
            if (HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric(formattedHouseNumber, formattedRrIndex))
            {
                var potentialMatch = MatchHouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric(formattedHouseNumber, formattedRrIndex);

                if (potentialMatch.Any())
                {
                    return potentialMatch;
                }
            }

            // RR-huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en zonder deel 4
            // Het RR-huisnummer+deel1 wordt een CRAB-huisnummer met numeriek bisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
            // Als deel2 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 het verdiepnummer van appartementnummer 0.
            // ***********************************************************************************************************************************************************************
            if (HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing(formattedHouseNumber, formattedRrIndex))
            {
                return MatchHouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing(formattedHouseNumber, formattedRrIndex);
            }

            // detecteer bereikachtige records
            // *******************************
            var potentialRange = MatchHouseNumberRanges(streetName, houseNumber, formattedRrIndex);
            if (potentialRange.Any())
            {
                return potentialRange;
            }

            // last fallback
            if (formattedRrIndex == "0000" && new Regex("[0-9]+").IsMatch(RemovePrecedingZeros(formattedHouseNumber)))
            {
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        new Regex("[0-9]+").Match(RemovePrecedingZeros(formattedHouseNumber)).Value,
                        null,
                        null)
                };
            }

            return new List<HouseNumberWithSubaddress>();
        }

        private FormattedAddressParts ParseHouseNumberFromStreetName(string streetName, string formattedHouseNumber, string formattedRrIndex)
        {
            var formattedParts = new FormattedAddressParts
            {
                HouseNumber = formattedHouseNumber,
                RrIndex = formattedRrIndex
            };

            var strippedStreetName = StripStreetName(streetName);
            var requestStraatnaamWithoutStraatnaam = string.Empty;

            if (!string.IsNullOrEmpty(strippedStreetName))
            {
                requestStraatnaamWithoutStraatnaam = streetName
                    .Replace(strippedStreetName, string.Empty);
            }

            requestStraatnaamWithoutStraatnaam = requestStraatnaamWithoutStraatnaam
                .Replace(",", string.Empty)
                .Replace(" ", string.Empty);

            var possibleNumberInStreet = FormatRrHouseNumber(LeftIndex(requestStraatnaamWithoutStraatnaam));
            var possibleIndexInStreet = FormatRrIndex(RightIndex(requestStraatnaamWithoutStraatnaam));

            // huisnummer als prefix in straatnaam
            if (new Regex("[0-9]+ ").IsMatch(streetName)
                && possibleIndexInStreet == "0000"
                && int.TryParse(possibleNumberInStreet, out int hnr)
                && hnr != 0)
            {
                formattedParts.HouseNumber = possibleNumberInStreet;
            }

            // huisnummer met niet-numeriek bisnummer als prefix in straatnaam
            if (new Regex("[0-9]+[a-zA-Z] ").IsMatch(streetName)
                && possibleNumberInStreet != null
                && int.TryParse(possibleNumberInStreet, out hnr)
                && hnr != 0
                && new Regex("[0-9]+[a-zA-Z]$").IsMatch(requestStraatnaamWithoutStraatnaam))
            {
                formattedParts.HouseNumber = possibleNumberInStreet + LeftIndex(possibleIndexInStreet).ToUpper();
                formattedParts.RrIndex = string.Empty;
            }

            // huisnummer als suffix in straatnaam
            if (new Regex(" [0-9]+$").IsMatch(streetName))
            {
                formattedParts.HouseNumber = FormatRrHouseNumber(new Regex(" [0-9]+$").Match(streetName).Value);

                Warnings.AddWarning("2", "'Huisnummer' in 'Straatnaam' gevonden.");
            }

            // huisnummer met niet-numeriek bisnummer als suffix in straatnaam
            if (new Regex(" [0-9]+[a-zA-Z]$").IsMatch(streetName)
                && possibleNumberInStreet != null
                && int.TryParse(possibleNumberInStreet, out hnr)
                && hnr != 0
                && new Regex("[0-9]+[a-zA-Z]$").IsMatch(requestStraatnaamWithoutStraatnaam))
            {
                formattedParts.HouseNumber = possibleNumberInStreet + LeftIndex(possibleIndexInStreet).ToUpper();
                formattedParts.RrIndex = string.Empty;

                Warnings.AddWarning("3", "Niet-numeriek 'Huisnummer' in 'Straatnaam' gevonden.");
            }

            return formattedParts;
        }

        private static bool HouseNumberWithoutIndex(string rrIndex)
            => rrIndex == "0000";

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithoutIndex(string houseNumber, bool strictMatch = false)
        {
            // volledig numerisch
            if (new Regex("^[0-9]+$").IsMatch(houseNumber))
            {
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber),
                        null,
                        null)
                };
            }

            // niet-numeriek bisnummer: enkele letter
            if (new Regex(@"^[0-9]+\s*[a-zA-Z]$").IsMatch(houseNumber))
            {
                var result = new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber[..^1].Trim()) + houseNumber.Last().ToString().ToUpper(),
                        null,
                        null)
                };

                if (!strictMatch)
                {
                    result.Add(new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber[..^1].Trim()),
                        null,
                        null));
                }

                return result;
            }

            // niet-numeriek bisnummer: 'bis'
            if (houseNumber.EndsWith("bis", StringComparison.InvariantCultureIgnoreCase) &&
                !new Regex("[a-zA-Z]").IsMatch(houseNumber[..^3]))
            {
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber[..^3]) + "BIS",
                        null,
                        null)
                };
            }

            return new List<HouseNumberWithSubaddress>();
        }

        private static bool HouseNumberWithIndex_Part1NonNumeric_Part2Zero(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               new Regex("^[a-zA-Z]").IsMatch(rrIndex) &&
               (RightIndex(rrIndex) == string.Empty || RemovePrecedingZeros(RightIndex(rrIndex)) == string.Empty);

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1NonNumeric_Part2Zero(string houseNumber, string rrIndex)
        {
            if (!IndexWordsWithoutBisNumber.Contains(LeftIndex(rrIndex)))
            {
                // niet-numeriek bisnummer
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber) + LeftIndex(rrIndex),
                        null,
                        null)
                };
            }

            // TODO: It seems this entire chunk is being ignored later, because AppNumber is never used. So basically, we always return the housenumber.

            // geen niet-numeriek bisnummer
            if (!IndexWordsFloorNumber.Contains(LeftIndex(rrIndex)))
            {
                // appartementnummer 0
                return IndexWordsFloor.Contains(LeftIndex(rrIndex))
                    ? new List<HouseNumberWithSubaddress>
                    {
                        new HouseNumberWithSubaddress(
                            RemovePrecedingZeros(houseNumber),
                            null,
                            "0.0")
                    }
                    : new List<HouseNumberWithSubaddress>
                    {
                        new HouseNumberWithSubaddress(
                            RemovePrecedingZeros(houseNumber),
                            null,
                            null)
                    };
            }

            return new List<HouseNumberWithSubaddress>
            {
                new HouseNumberWithSubaddress(
                    RemovePrecedingZeros(houseNumber),
                    null,
                    null)
            };
        }

        private static bool HouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               new Regex("^[a-zA-Z]").IsMatch(rrIndex) &&
               int.TryParse(RightIndex(rrIndex), out var bnr) &&
               bnr > 0;

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero(string houseNumber, string rrIndex)
        {
            if (!IndexWordsWithoutBisNumber.Contains(LeftIndex(rrIndex)))
            {
                //niet-numeriek bisnummer
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber) + LeftIndex(rrIndex),
                        RemovePrecedingZeros(RightIndex(rrIndex)),
                        null)
                };
            }

            // geen niet-numeriek bisnummer
            // note: appnumber is not used anywhere later...
            return IndexWordsBoxNumber.Contains(LeftIndex(rrIndex))
                ? new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber),
                        RemovePrecedingZeros(RightIndex(rrIndex)),
                        null)
                }
                : new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber),
                        null,
                        LeftIndex(rrIndex) + RemovePrecedingZeros(RightIndex(rrIndex)))
                };
        }

        private static bool HouseNumberWithIndex_Part1StartNumber_Part3Missing(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               !string.IsNullOrEmpty(rrIndex) &&
               rrIndex != "0000" &&
               new Regex("^[0-9]").IsMatch(rrIndex) &&
               LeftIndex(RightIndex(rrIndex)) == string.Empty;

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1StartNumber_Part3Missing(string houseNumber, string rrIndex)
        {
            return new List<HouseNumberWithSubaddress>
            {
                new HouseNumberWithSubaddress(
                    RemovePrecedingZeros(houseNumber),
                    RemovePrecedingZeros(LeftIndex(rrIndex)),
                    null)
            };
        }

        private static bool HouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               !string.IsNullOrEmpty(rrIndex) &&
               rrIndex != "0000" &&
               new Regex("^[0-9]").IsMatch(rrIndex) &&
               int.TryParse(LeftIndex(RightIndex(rrIndex)), out _) &&
               RightIndex(RightIndex(rrIndex)) == string.Empty;

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing(string houseNumber, string rrIndex)
        {
            return new List<HouseNumberWithSubaddress>
            {
                new HouseNumberWithSubaddress(
                    RemovePrecedingZeros(houseNumber),
                    null,
                    rrIndex)
            };
        }

        private static bool HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               !string.IsNullOrEmpty(rrIndex) &&
               rrIndex != "0000" &&
               new Regex("^[a-zA-Z]").IsMatch(LeftIndex(RightIndex(rrIndex))) &&
               int.TryParse(RightIndex(RightIndex(rrIndex)), out _);

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric(string houseNumber, string rrIndex)
        {
            if (!IndexWordsWithoutBisNumber.Contains(LeftIndex(RightIndex(rrIndex))))
            {
                // niet-numeriek bisnummer
                var bus = RemovePrecedingZeros(RightIndex(RightIndex(rrIndex))).PadLeft(1, '0');
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber) + LeftIndex(RightIndex(rrIndex)),
                        bus,
                        null)
                };
            }

            if (IndexWordsFloorNumber.Contains(LeftIndex(RightIndex(rrIndex))))
            {
                // verdiepnummer
                var part1 = RemovePrecedingZeros(LeftIndex(rrIndex)).PadLeft(1, '0');
                var part2 = RemovePrecedingZeros(RightIndex(RightIndex(rrIndex))).PadLeft(1, '0');
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber),
                        null,
                        $"{part1}.{part2}")
                };
            }

            if (IndexWordsAppartementNumber.Contains(LeftIndex(RightIndex(rrIndex))))
            {
                // appartementnummer
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber) + RemovePrecedingZeros(LeftIndex(rrIndex)),
                        null,
                        RemovePrecedingZeros(RightIndex(RightIndex(rrIndex))))
                };
            }

            if (IndexWordsBoxNumber.Contains(LeftIndex(RightIndex(rrIndex))))
            {
                // busnummer
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber) + RemovePrecedingZeros(LeftIndex(rrIndex)),
                        RemovePrecedingZeros(RightIndex(RightIndex(rrIndex))),
                        null)
                };
            }

            return new List<HouseNumberWithSubaddress>();
        }

        private static bool HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing(string houseNumber, string rrIndex)
            => int.TryParse(houseNumber, out _) &&
               !string.IsNullOrEmpty(rrIndex) &&
               rrIndex != "0000" &&
               new Regex("^[0-9]").IsMatch(rrIndex) &&
               new Regex("^[a-zA-Z]").IsMatch(LeftIndex(RightIndex(rrIndex))) &&
               RightIndex(RightIndex(rrIndex)) == string.Empty;

        private static List<HouseNumberWithSubaddress> MatchHouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing(string houseNumber, string rrIndex)
        {
            if (IndexWordsFloorNumber.Contains(RightIndex(rrIndex)))
            {
                // verdiepnummer
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        RemovePrecedingZeros(houseNumber),
                        null,
                        $"{RemovePrecedingZeros(LeftIndex(rrIndex))}.0")
                };
            }

            // geen verdiepnummer
            return new List<HouseNumberWithSubaddress>
            {
                new HouseNumberWithSubaddress(
                    $"{RemovePrecedingZeros(houseNumber)}_{RemovePrecedingZeros(LeftIndex(rrIndex))}",
                    RightIndex(rrIndex),
                    null)
            };
        }

        private static List<HouseNumberWithSubaddress> MatchHouseNumberRanges(string? streetName, string? houseNumber, string rrindex0)
        {
            string? houseNumber1 = null;
            string? houseNumber2 = null;

            if (rrindex0 == "0000" || IndexWordsWithoutBisNumber.Contains(rrindex0))
            {
                if (houseNumber != null && houseNumber.Contains("-") && int.TryParse(houseNumber.Split('-')[0].Trim(), out _))
                {
                    houseNumber1 = houseNumber.Split('-')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('-') + 1).Trim();
                }

                if (houseNumber != null && houseNumber.Contains("/") && int.TryParse(houseNumber.Split('/')[0].Trim(), out _))
                {
                    houseNumber1 = houseNumber.Split('/')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('/') + 1).Trim();
                }

                if (houseNumber != null && houseNumber.Contains("+") && int.TryParse(houseNumber.Split('+')[0].Trim(), out _))
                {
                    houseNumber1 = houseNumber.Split('+')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf('+') + 1).Trim();
                }

                if (houseNumber != null && new Regex("[0-9]+ [0-9]+").IsMatch(houseNumber) &&
                    int.TryParse(houseNumber.Split(' ')[0].Trim(), out _))
                {
                    houseNumber1 = houseNumber.Split(' ')[0].Trim();
                    houseNumber2 = houseNumber.Substring(houseNumber.IndexOf(' ') + 1).Trim();
                }

                if (streetName != null && new Regex("[0-9]+-[0-9]+").IsMatch(streetName))
                {
                    var matchedRange = new Regex("[0-9]+-[0-9]+").Match(streetName);
                    houseNumber1 = matchedRange.Value.Split('-')[0];
                    houseNumber2 = matchedRange.Value.Substring(matchedRange.Value.IndexOf('-') + 1);
                }
            }

            if (houseNumber1 == null)
            {
                return new List<HouseNumberWithSubaddress>();
            }

            // omzetten van bereik naar adres
            if (!int.TryParse(houseNumber2, out var hnr))
            {
                // niet-numeriek bisnummer genoteerd als bereik
                if (new Regex("[a-zA-Z]").IsMatch(houseNumber2))
                {
                    return new List<HouseNumberWithSubaddress>
                    {
                        new HouseNumberWithSubaddress(
                            houseNumber1,
                            houseNumber2.ToUpper(),
                            null)
                    };
                }

                // enkel huisnummer genoteerd als bereik
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        houseNumber1,
                        null,
                        null)
                };
            }

            // subadres genoteerd als bereik
            if (int.Parse(houseNumber1) > hnr)
            {
                return new List<HouseNumberWithSubaddress>
                {
                    new HouseNumberWithSubaddress(
                        houseNumber1,
                        houseNumber2,
                        null)
                };
            }

            return PopulateHouseNumberRange(houseNumber1, houseNumber2);
        }

        private static List<HouseNumberWithSubaddress> PopulateHouseNumberRange(
            string? from,
            string? to)
        {
            if (from == null || to == null)
            {
                return new List<HouseNumberWithSubaddress>();
            }

            if (!int.TryParse(from, out var fromHouseNumber))
            {
                return new List<HouseNumberWithSubaddress>();
            }

            if (!int.TryParse(to, out var toHouseNumber))
            {
                return new List<HouseNumberWithSubaddress>();
            }

            static bool IsEven(int number) => number % 2 == 0;
            static bool IsOdd(int number) => number % 2 != 0;
            static IEnumerable<int> RangedEnumeration(int min, int max, int step)
                => Enumerable.Range(min, max - min + 1).Where(i => (i - min) % step == 0);

            return (fromHouseNumber, toHouseNumber) switch
            {
                var (f, t) when IsEven(f) && IsEven(t) || IsOdd(f) && IsOdd(t) =>
                    RangedEnumeration(f, t, 2)
                        .Select(x => new HouseNumberWithSubaddress(x.ToString(), null, null))
                        .ToList(),

                var (f, t) =>
                    RangedEnumeration(f, t, 1)
                        .Select(x => new HouseNumberWithSubaddress(x.ToString(), null, null))
                        .ToList()
            };
        }

        private static string FormatRrHouseNumber(string? rrHouseNumber)
            => string.IsNullOrEmpty(rrHouseNumber)
                ? string.Empty
                : rrHouseNumber.Trim().PadLeft(8, '0');

        private static string FormatRrIndex(string? rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
            {
                return "0000";
            }

            var formatted = RemovePrecedingZeros(rrIndex).Trim();

            return ContainsNumber.IsMatch(formatted)
                ? formatted.PadLeft(4, '0')
                : formatted.PadRight(4, '0');
        }

        private static string StripStreetName(string streetName)
        {
            //TODO: optimize
            string trimmed = streetName;

            // straatnaam, hnr [busnr]
            if (trimmed.Contains(","))
            {
                trimmed = trimmed.Substring(0, trimmed.IndexOf(",", StringComparison.Ordinal));
            }

            var match = ContainsNumber.Match(trimmed);
            if (match.Length <= 0)
            {
                return trimmed;
            }

            // hrn[busnr] straatnaam
            if (match.Index == 0)
            {
                trimmed = trimmed.Replace(match.Value, string.Empty);
                while (!trimmed.StartsWith(" ") && trimmed.Length > 0)
                {
                    trimmed = trimmed[1..];
                }

                trimmed = trimmed.Trim();
            }

            // straatnaam hnr [busnr]
            return match.Index <= 0
                ? trimmed
                : trimmed.Substring(0, trimmed.Length - (trimmed.Length - match.Index)).Trim();
        }

        private static string RightIndex(string rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
            {
                return string.Empty;
            }

            var rightPart = string.Empty;

            if (Begin.IsMatch(rrIndex))
            {
                rightPart = rrIndex.Substring(Begin.Match(rrIndex).Value.Length);
            }

            while (!Begin.IsMatch(rightPart) && rightPart.Length > 0)
            {
                rightPart = rightPart.Substring(1);
            }

            return rightPart;
        }

        private static string LeftIndex(string rrIndex)
        {
            if (string.IsNullOrEmpty(rrIndex))
            {
                return string.Empty;
            }

            var leftPart = string.Empty;
            if (Begin.IsMatch(rrIndex))
            {
                leftPart = Begin.Match(rrIndex).Value;
            }

            return leftPart;
        }

        private static string RemovePrecedingZeros(string input)
            => string.IsNullOrEmpty(input)
                ? string.Empty
                : input.TrimStart(new[] { '0' });

        private sealed class FormattedAddressParts
        {
            public string? HouseNumber { get; set; }
            public string? RrIndex { get; set; }
        }
    }
}
