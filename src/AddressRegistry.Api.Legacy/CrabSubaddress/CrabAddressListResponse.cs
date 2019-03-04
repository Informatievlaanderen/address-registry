namespace AddressRegistry.Api.Legacy.CrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using CrabHouseNumber;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract(Name = "CrabSubadresCollectie", Namespace = "")]
    public class CrabSubAddressListResponse
    {
        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [DataMember(Name = "CrabSubadressen", Order = 1)]
        public List<CrabSubAddressListItem> Addresses { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        [DataMember(Name = "TotaalAantal", Order = 2)]
        public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "CrabSubadresCollectieItem", Namespace = "")]
    public class CrabSubAddressListItem
    {
        public CrabSubAddressListItem(
            int crabSubadresId,
            int osloId,
            string naamruimte,
            string detailUrl,
            string houseNumber,
            string boxNumber,
            VolledigAdres volledigAdres,
            DateTimeOffset version,
            bool isComplete)
        {
            CrabSubadresId = crabSubadresId;
            Address = new CrabAddressListItemAddress
            {
                Identificator = new Identificator(naamruimte, osloId.ToString(CultureInfo.InvariantCulture), version),
                IsComplete = isComplete,
                Detail = new Uri(detailUrl),
                Huisnummer = houseNumber,
                Busnummer = boxNumber,
                VolledigAdres = volledigAdres,
            };
        }

        /// <summary>
        /// Het CRAB HuisnummerId.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        public int CrabSubadresId { get; set; }

        /// <summary>
        ///	Het corresponderend address.
        /// </summary>
        [DataMember(Name = "Adres", Order = 2)]
        public CrabAddressListItemAddress Address { get; set; }
    }
}
