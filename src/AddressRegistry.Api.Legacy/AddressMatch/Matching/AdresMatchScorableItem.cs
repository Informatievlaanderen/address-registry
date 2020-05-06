namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Responses;

    public class AdresMatchScorableItem : IScoreable
    {
        public Guid AddressId { get; set; }
        public AdresIdentificator Identificator { get; set; }
        public string Detail { get; set; }
        public AdresMatchItemGemeente Gemeente { get; set; }
        public AdresMatchItemPostinfo Postinfo { get; set; }
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

        public string ScoreableProperty
        {
            get
            {
                if (VolledigAdres != null)
                    return VolledigAdres.GeografischeNaam?.Spelling;
                else if (Gemeente != null && Straatnaam != null)
                    return $"{Straatnaam.Straatnaam?.GeografischeNaam?.Spelling}, {Gemeente.Gemeentenaam?.GeografischeNaam?.Spelling}";
                else if (Gemeente != null)
                    return Gemeente.Gemeentenaam?.GeografischeNaam?.Spelling;
                else
                    return null;
            }
        }
    }
}
