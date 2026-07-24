namespace AddressRegistry.Api.Oslo.Address.V3.Search
{
    using System;
    using System.Collections.Generic;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class AddressSearchV3Response
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "resultaten", Order = 0, Required = Required.DisallowNull)]
        public List<AddressSearchItemV3> Results { get; set; }

        public AddressSearchV3Response(List<AddressSearchItemV3> results)
        {
            Results = results;
        }
    }

    public sealed class AddressSearchItemV3
    {
        /// <summary>
        /// De unieke en persistente identificator van het object (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De objectidentificator (enkel uniek binnen naamruimte).
        /// </summary>
        [JsonProperty(PropertyName = "objectId", Order = 2, Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die naar de details van de meeste recente versie van een enkel adres leidt.
        /// </summary>
        [JsonProperty(PropertyName = "detail", Order = 3, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// Textueel resultaat van de zoekopdracht (straatnaam of volledig adres).
        /// </summary>
        [JsonProperty(PropertyName = "resultaat", Order = 4, Required = Required.DisallowNull)]
        public string Result { get; set; }

        public AddressSearchItemV3(string id, string objectId, Uri detail, string result)
        {
            Id = id;
            ObjectId = objectId;
            Detail = detail;
            Result = result;
        }
    }

    public class AddressSearchResponseExamples : IExamplesProvider<AddressSearchV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public AddressSearchResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressSearchV3Response GetExamples()
        {
            var addressExamples = new List<AddressSearchItemV3>
            {
                new AddressSearchItemV3(
                    "https://data.vlaanderen.be/id/adres/1",
                    "1",
                    new Uri(string.Format(_responseOptions.DetailUrl, "1")),
                    "zoekresultaat 1"),
                new AddressSearchItemV3(
                    "https://data.vlaanderen.be/id/adres/2",
                    "2",
                    new Uri(string.Format(_responseOptions.DetailUrl, "2")),
                    "zoekresultaat 2"),
            };

            return new AddressSearchV3Response(addressExamples);
        }
    }
}
