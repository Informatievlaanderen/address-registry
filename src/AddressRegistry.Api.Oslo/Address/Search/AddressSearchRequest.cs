namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Newtonsoft.Json;

    public sealed record AddressSearchRequest(
        FilteringHeader<AddressSearchFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<AddressSearchResponse>;

    public sealed record AddressSearchResponse(IEnumerable<AddressSearchItem> Items);

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

    public sealed class AddressSearchFilter
    {
        public string Query { get; init; }
    }
}
