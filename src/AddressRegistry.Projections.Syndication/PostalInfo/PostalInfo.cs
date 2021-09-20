namespace AddressRegistry.Projections.Syndication.PostalInfo
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "PostInfo", Namespace = "")]
    public class PostalInfo
    {
        /// <summary>
        /// De technische id van de postinfo.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string PostalCode { get; set; }

        /// <summary>
        /// De identificator van de postcode.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public PostinfoIdentificator Identificator { get; set; }

        /// <summary>
        /// De namen van het gebied dat de postcode beslaat, in de taal afkomstig uit het bPost bestand.
        /// </summary>
        [DataMember(Name = "Postnamen", Order = 2)]
        public List<Postnaam> PostalNames { get; set; }

        /// <summary>
        /// De huidige fase in de doorlooptijd van de postcode.
        /// </summary>
        [DataMember(Name = "PostInfoStatus", Order = 3)]
        public PostInfoStatus? Status { get; set; }

        /// <summary>
        /// De NisCode van de gemeente waarmee de postcode verwant is.
        /// </summary>
        [DataMember(Name = "NisCode", Order = 3)]
        public string MunicipalityNisCode { get; set; }

        public PostalInfo()
        {
            PostalNames = new List<Postnaam>();
        }
    }
}
