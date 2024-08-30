namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure.Options;
    using List;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "ZoekResultaten", Namespace = "")]
    public sealed class AddressSearchResponse
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [DataMember(Name = "Resultaten", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<AddressSearchItem> Results { get; set; }

        public AddressSearchResponse(List<AddressSearchItem> results)
        {
            Results = results;
        }
    }

    public sealed class AddressSearchItem
    {
        /// <summary>
        /// De unieke en persistente identificator van het object (volgt de Vlaamse URI-standaard).
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De objectidentificator (enkel uniek binnen naamruimte).
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkel adres leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Textueel resultaat van de zoekopdracht (straatnaam of volledig adres).
        /// </summary>
        [DataMember(Name = "Resultaat", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Result { get; set; }

        public AddressSearchItem(string id, string objectId, Uri detail, string result)
        {
            Id = id;
            ObjectId = objectId;
            Detail = detail;
            Result = result;
        }
    }

    public class AddressSearchResponseExamples : IExamplesProvider<AddressSearchResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressSearchResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressSearchResponse GetExamples()
        {
            var addressExamples = new List<AddressSearchItem>
            {
                new AddressSearchItem(
                    "https://data.vlaanderen.be/id/adres/1",
                    "1",
                    new Uri(string.Format(_responseOptions.DetailUrl, "1")),
                    "zoekresultaat 1"),
                new AddressSearchItem(
                    "https://data.vlaanderen.be/id/adres/2",
                    "2",
                    new Uri(string.Format(_responseOptions.DetailUrl, "2")),
                    "zoekresultaat 2"),
            };

            return new AddressSearchResponse(addressExamples);
        }
    }
}
