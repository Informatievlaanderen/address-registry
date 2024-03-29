namespace AddressRegistry.Api.Legacy.AddressMatch.V2.Matching
{
    using System;
    using Responses;
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;

    public class AddressMatchScoreableItemV2 : IScoreable
    {
        public int AddressPersistentLocalId { get; set; }
        public AdresIdentificator Identificator { get; set; }
        public string Detail { get; set; }
        public AdresMatchItemGemeente Gemeente { get; set; }
        public AdresMatchItemPostinfo? Postinfo { get; set; }
        public AdresMatchItemStraatnaam Straatnaam { get; set; }
        public HomoniemToevoeging HomoniemToevoeging { get; set; }
        public string Huisnummer { get; set; }
        public string Busnummer { get; set; }
        public VolledigAdres VolledigAdres { get; set; }
        public Point AdresPositie { get; set; }
        public PositieSpecificatie? PositieSpecificatie { get; set; }
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }
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
