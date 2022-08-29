namespace AddressRegistry.Projections.Syndication.AddressLink
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;

    [DataContract(Name = "Adres", Namespace = "")]
    public class Address
    {
        /// <summary>
        /// De technische id van het adres.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string AddressId { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De id van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 3)]
        public string? SteetnameId { get; set; }

        /// <summary>
        /// De id van de postinfo.
        /// </summary>
        [DataMember(Name = "PostCode", Order = 4)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 5)]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 6)]
        public string BoxNumber { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 7)]
        public AdresStatus? AdressStatus { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }
    }

    public enum AddressEvent
    {
        AddressWasRegistered,

        AddressBecameComplete,
        AddressBecameIncomplete,

        AddressBecameCurrent,
        AddressBecameNotOfficiallyAssigned,
        AddressOfficialAssignmentWasRemoved,
        AddressStatusWasCorrectedToRemoved,
        AddressStatusWasRemoved,
        AddressWasCorrectedToCurrent,
        AddressWasCorrectedToNotOfficiallyAssigned,
        AddressWasCorrectedToOfficiallyAssigned,
        AddressWasCorrectedToProposed,
        AddressWasCorrectedToRetired,
        AddressWasOfficiallyAssigned,
        AddressWasProposed,
        AddressWasRetired,

        AddressBoxNumberWasChanged,
        AddressBoxNumberWasCorrected,
        AddressBoxNumberWasRemoved,

        AddressHouseNumberWasChanged,
        AddressHouseNumberWasCorrected,

        AddressPositionWasCorrected,
        AddressPositionWasRemoved,
        AddressWasPositioned,

        AddressPostalCodeWasChanged,
        AddressPostalCodeWasCorrected,
        AddressPostalCodeWasRemoved,

        AddressStreetNameWasChanged,
        AddressStreetNameWasCorrected,

        AddressWasRemoved,
        AddressPersistentLocalIdentifierWasAssigned,
    }
}
