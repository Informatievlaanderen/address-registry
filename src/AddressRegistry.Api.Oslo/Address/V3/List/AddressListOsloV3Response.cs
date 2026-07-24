namespace AddressRegistry.Api.Oslo.Address.V3.List
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class AddressListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de adressenverzameling.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "AdressenEnvelop";

        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public List<AddressListItemOsloV3Response> Adressen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [JsonProperty(PropertyName = "volgende", Order = 2, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Volgende { get; set; }

        [JsonIgnore]
        public SortingHeader Sorting { get; set; }

        [JsonIgnore]
        public PaginationInfo Pagination { get; set; }
    }

    public class AddressListItemOsloV3Response
    {
        /// <summary>
        /// Het linked-data type van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Adres";

        /// <summary>
        /// De unieke en persistente identificator van het adres (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkel adres leidt.
        /// </summary>
        [JsonProperty(PropertyName = "detail", Order = 2, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "huisnummer", Order = 3, Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "busnummer", Order = 4, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string? Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [JsonProperty(PropertyName = "isVerrijktMet", Order = 5, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 6, Required = Required.DisallowNull)]
        public AdresStatus AdresStatus { get; set; }

        public AddressListItemOsloV3Response(
            int id,
            string detail,
            string huisnummer,
            string? busnummer,
            VolledigAdres volledigAdres,
            AdresStatusValue status,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Adres.ToPuri(id.ToString());
            Identificator = new AdresIdentificator(id.ToString(), version);
            Detail = new Uri(string.Format(detail, id));
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            VolledigAdres = volledigAdres;
            AdresStatus = new AdresStatus(status);
        }
    }

    public class AddressListOsloResponseExamples : IExamplesProvider<AddressListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public AddressListOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressListOsloV3Response GetExamples()
        {
            var volAdres1 = new VolledigAdres();
            volAdres1.Add("Koningin Maria Hendrikaplein", "70", string.Empty, "9000", "Gent", Taal.Nl);

            var volAdres2 = new VolledigAdres();
            volAdres2.Add("Boudewijnlaan", "30", "30", "1000", "Brussel", Taal.Nl);
            var addressExamples = new List<AddressListItemOsloV3Response>
            {
                new AddressListItemOsloV3Response(
                    10521,
                    _responseOptions.DetailUrl,
                    "70",
                    null,
                    volAdres1,
                    AdresStatusValue.Voorgesteld,
                    DateTimeOffset.Now.ToExampleOffset()),
                new AddressListItemOsloV3Response(
                    14874,
                    _responseOptions.DetailUrl,
                    "30",
                    "30",
                    volAdres2,
                    AdresStatusValue.InGebruik,
                    DateTimeOffset.Now.AddDays(-2).ToExampleOffset())
            };

            return new AddressListOsloV3Response
            {
                Adressen = addressExamples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
