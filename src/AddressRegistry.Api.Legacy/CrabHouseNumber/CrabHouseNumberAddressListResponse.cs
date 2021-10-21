namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CrabHuisnummerCollectie", Namespace = "")]
    public class CrabHouseNumberAddressListResponse
    {
        /// <summary>
        /// De verzameling van CRAB huisnummers.
        /// </summary>
        [DataMember(Name = "CrabHuisnummers", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<CrabHouseNumberAddressListItem> Addresses { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 2)]
        //[JsonProperty(Required = Required.DisallowNull)]
        //public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "CrabHuisnummer", Namespace = "")]
    public class CrabHouseNumberAddressListItem
    {
        /// <summary>
        /// De CRAB huisnummerid.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int CrabHouseNumberId { get; set; }

        /// <summary>
        ///	Het corresponderend address.
        /// </summary>
        [DataMember(Name = "Adres", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public CrabAddressListItemAddress Address { get; set; }

        public CrabHouseNumberAddressListItem(
            int crabHouseNumberId,
            int persistentLocalId,
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
                Identificator = new AdresIdentificator(naamruimte, persistentLocalId.ToString(CultureInfo.InvariantCulture), version),
                IsComplete = isComplete,
                Detail = new Uri(string.Format(detailUrl, persistentLocalId)),
                Huisnummer = houseNumber,
                VolledigAdres = volledigAdres,
            };
        }
    }

    /// <summary>
    ///	Het corresponderende adres uit het Adressenregister.
    /// </summary>
    [DataContract(Name = "CrabHuisnummerSubadresAdres", Namespace = "")]
    public class CrabAddressListItemAddress
    {
        /// <summary>
        /// True als het adres volledig is (heeft status en positie).
        /// </summary>
        [DataMember(Name = "IsVolledig", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkele straatnaam leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres. 
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }
    }

    public class CrabHouseNumberListResponseExamples : IExamplesProvider<CrabHouseNumberAddressListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public CrabHouseNumberListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public CrabHouseNumberAddressListResponse GetExamples()
        {
            var addressExamples = new List<CrabHouseNumberAddressListItem>
            {
                new CrabHouseNumberAddressListItem(
                    5,
                    10521,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "70",
                    new VolledigAdres("Koningin Maria Hendrikaplein", "70", null, "9000", "Gent", Taal.NL),
                    DateTimeOffset.Now.ToExampleOffset(),
                    true),
                new CrabHouseNumberAddressListItem(
                    157,
                    14874,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "30",
                    new VolledigAdres("Boudewijnlaan", "30", "30", "1000", "Brussel", Taal.NL),
                    DateTimeOffset.Now.AddDays(-2).ToExampleOffset(),
                    false)
            };

            return new CrabHouseNumberAddressListResponse
            {
                Addresses = addressExamples,
                Volgende = new Uri(string.Format(_responseOptions.CrabHuisnummersVolgendeUrl, 2, 10))
            };
        }
    }
}
