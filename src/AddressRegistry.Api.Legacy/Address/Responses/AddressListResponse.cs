namespace AddressRegistry.Api.Legacy.Address.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "AdresCollectie", Namespace = "")]
    public class AddressListResponse
    {
        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [DataMember(Name = "Adressen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<AddressListItemResponse> Adressen { get; set; }

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
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "Adres", Namespace = "")]
    public class AddressListItemResponse
    {
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
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// De voorstelling van een adres in het Nederlands.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }

        public AddressListItemResponse(
            int id,
            string naamruimte,
            string detail,
            string huisnummer,
            string busnummer,
            VolledigAdres volledigAdres,
            DateTimeOffset version)
        {
            Identificator = new AdresIdentificator(naamruimte, id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            VolledigAdres = volledigAdres;
        }
    }

    public class AddressListResponseExamples : IExamplesProvider<AddressListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressListResponse GetExamples()
        {
            var addressExamples = new List<AddressListItemResponse>
            {
                new AddressListItemResponse(
                    10521,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "70",
                    null,
                    new VolledigAdres("Koningin Maria Hendrikaplein", "70", null, "9000", "Gent", Taal.NL),
                    DateTimeOffset.Now),
                new AddressListItemResponse(
                    14874,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "30",
                    "30",
                    new VolledigAdres("Boudewijnlaan", "30", "30", "1000", "Brussel", Taal.NL),
                    DateTimeOffset.Now.AddDays(-2))
            };

            return new AddressListResponse
            {
                Adressen = addressExamples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
