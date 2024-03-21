namespace AddressRegistry.Api.Oslo.AddressMatch.V1.Matching
{
    using System;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Responses;

    public class AdresMatchScorableItem : IScoreable
    {
        public Guid AddressId { get; set; }
        public AdresIdentificator Identificator { get; set; }
        public string Detail { get; set; }
        public AdresMatchOsloItemGemeente Gemeente { get; set; }
        public AdresMatchOsloItemPostinfo? Postinfo { get; set; }
        public AdresMatchOsloItemStraatnaam Straatnaam { get; set; }
        public HomoniemToevoeging HomoniemToevoeging { get; set; }
        public string Huisnummer { get; set; }
        public string Busnummer { get; set; }
        public VolledigAdres VolledigAdres { get; set; }
        public  AddressPosition AdresPositie { get; set; }
        public AdresStatus? AdresStatus { get; set; }
        public bool? OfficieelToegekend { get; set; }
        public double Score { get; set; }

        public string? ScoreableProperty
        {
            get
            {
                if (VolledigAdres != null)
                {
                    var completeAddress = VolledigAdres.GeografischeNaam?.Spelling;

                    if (!string.IsNullOrWhiteSpace(Huisnummer))
                    {
                        completeAddress = completeAddress?.ReplaceFirst(
                            Huisnummer,
                            Huisnummer.PadLeft(10, '0'),
                            StringComparison.OrdinalIgnoreCase);
                    }

                    if (!string.IsNullOrWhiteSpace(Busnummer))
                    {
                        completeAddress = completeAddress?.ReplaceFirst(
                            $"{Busnummer},",
                            $"{Busnummer.PadLeft(10, '0')},",
                            StringComparison.OrdinalIgnoreCase);
                    }

                    return completeAddress;
                }

                if (Gemeente != null && Straatnaam != null)
                    return $"{Straatnaam.Straatnaam?.GeografischeNaam?.Spelling}, {Gemeente.Gemeentenaam?.GeografischeNaam?.Spelling}";

                return Gemeente?.Gemeentenaam?.GeografischeNaam?.Spelling;
            }
        }
    }
}
