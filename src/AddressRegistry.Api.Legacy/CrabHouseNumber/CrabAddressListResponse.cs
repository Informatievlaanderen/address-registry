namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract(Name = "CrabHuisnummerCollectie", Namespace = "")]
    public class CrabAddressListResponse
    {
        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [DataMember(Name = "CrabHuisnummers", Order = 1)]
        public List<CrabAddressListItem> Addresses { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        [DataMember(Name = "TotaalAantal", Order = 2)]
        public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "CrabHuisnummerCollectieItem", Namespace = "")]
    public class CrabAddressListItem
    {
        public CrabAddressListItem(
            int crabHouseNumberId,
            int osloId,
            string naamruimte,
            string detailUrl,
            string houseNumber,
            VolledigAdres volledigAdres,
            DateTimeOffset version,
            bool isComplete)
        {
            CrabHouseNumberId = crabHouseNumberId;
            Address = new CrabAddressListItemAddress
            {
                Identificator = new Identificator(naamruimte, osloId.ToString(CultureInfo.InvariantCulture), version),
                IsComplete = isComplete,
                Detail = new Uri(detailUrl),
                Huisnummer = houseNumber,
                VolledigAdres = volledigAdres,
            };
        }

        /// <summary>
        /// Het CRAB HuisnummerId.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        public int CrabHouseNumberId { get; set; }

        /// <summary>
        ///	Het corresponderend address.
        /// </summary>
        [DataMember(Name = "Adres", Order = 2)]
        public CrabAddressListItemAddress Address { get; set; }
    }

    [DataContract(Name = "CrabHuisnummerSubadresAdres", Namespace = "")]
    public class CrabAddressListItemAddress
    {
        /// <summary>
        /// True als het adres volledig is (heeft status en positie).
        /// </summary>
        [DataMember(Name = "IsVolledig", Order = 1)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkele straatnaam leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 3)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 4)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het nummer van de bus.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 5)]
        public string Busnummer { get; set; }

        /// <summary>
        /// De voorstelling van een adres in het Nederlands.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 6)]
        public VolledigAdres VolledigAdres { get; set; }
    }
}
