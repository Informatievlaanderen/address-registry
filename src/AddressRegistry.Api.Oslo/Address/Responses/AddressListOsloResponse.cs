namespace AddressRegistry.Api.Oslo.Address.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;

    [DataContract(Name = "AdresCollectie", Namespace = "")]
    public class AddressListOsloResponse
    {
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context =>
            @"https://raw.githubusercontent.com/Informatievlaanderen/OSLOthema-gebouwEnAdres/d44fbba69aeb9f02d10d4e372449c404f3ebd06c/site-skeleton/adressenregister/context/adressen_list.jsonld";

        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [DataMember(Name = "Adressen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<AddressListItemOsloResponse> Adressen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "Adres", Namespace = "")]
    public class AddressListItemOsloResponse
    {
        /// <summary>
        /// Het linked-data type van het Adres.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Adres";

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkel adres leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 5)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VolledigAdres? VolledigAdres { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresStatus AdresStatus { get; set; }

        public AddressListItemOsloResponse(
            int id,
            string naamruimte,
            string detail,
            string huisnummer,
            string busnummer,
            VolledigAdres volledigAdres,
            AdresStatus status,
            DateTimeOffset version)
        {
            Identificator = new AdresIdentificator(naamruimte, id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            VolledigAdres = volledigAdres;
            AdresStatus = status;
        }
    }

    public class AddressListOsloResponseExamples : IExamplesProvider<AddressListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressListOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressListOsloResponse GetExamples()
        {
            var addressExamples = new List<AddressListItemOsloResponse>
            {
                new AddressListItemOsloResponse(
                    10521,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "70",
                    null,
                    new VolledigAdres("Koningin Maria Hendrikaplein", "70", null, "9000", "Gent", Taal.NL),
                    AdresStatus.Voorgesteld,
                    DateTimeOffset.Now.ToExampleOffset()),
                new AddressListItemOsloResponse(
                    14874,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "30",
                    "30",
                    new VolledigAdres("Boudewijnlaan", "30", "30", "1000", "Brussel", Taal.NL),
                    AdresStatus.InGebruik,
                    DateTimeOffset.Now.AddDays(-2).ToExampleOffset())
            };

            return new AddressListOsloResponse
            {
                Adressen = addressExamples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
