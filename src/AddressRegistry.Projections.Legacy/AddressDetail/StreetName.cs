namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;

    [DataContract(Name = "Straatnaam", Namespace = "")]
    public class StreetName
    {
        /// <summary>
        /// De technische id van de straatnaam.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid StreetNameId { get; set; }

        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De officiële namen van de straatnaam.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 3)]
        public List<GeografischeNaam> StreetNames { get; set; }

        /// <summary>
        /// De huidige fase in het leven van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamStatus", Order = 4)]
        public StraatnaamStatus? StreetNameStatus { get; set; }

        /// <summary>
        /// De homoniem-toevoegingen aan de straatnaam in verschillende talen.
        /// </summary>
        [DataMember(Name = "HomoniemToevoegingen", Order = 5)]
        public List<GeografischeNaam> HomonymAdditions { get; set; }

        /// <summary>
        /// De NisCode van de gerelateerde gemeente.
        /// </summary>
        [DataMember(Name = "NisCode", Order = 6)]
        public string NisCode { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 7)]
        public bool IsComplete { get; set; }
    }
}
