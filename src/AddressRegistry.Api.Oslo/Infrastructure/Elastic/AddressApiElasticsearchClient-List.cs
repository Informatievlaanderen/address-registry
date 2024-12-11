namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Convertors;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Core.Search;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Projections.Elastic.AddressSearch;

    public sealed partial class AddressApiElasticsearchClient
    {
        public async Task<AddressSearchResult> ListAddresses(
            string? streetNameId,
            string? streetName,
            string? homonymAddition,
            string? houseNumber,
            string? boxNumber,
            string? postalCode,
            string? nisCode,
            string? municipalityName,
            string? status,
            int? from,
            int? size)
        {
            if (!string.IsNullOrEmpty(status) & !Enum.TryParse(typeof(AdresStatus), status, true, out var parsedStatus))
            {
                return new AddressSearchResult(Enumerable.Empty<AddressSearchDocument>().ToList(), 0);
            }

            var searchResponse = await ElasticsearchClient.SearchAsync<AddressSearchDocument>(IndexAlias, descriptor =>
            {
                descriptor.Size(size);
                descriptor.From(from);
                descriptor.TrackTotalHits(new TrackHits(true));
                descriptor.Sort(new List<SortOptions>
                {
                    SortOptions.Field(new Field(ToCamelCase(nameof(AddressSearchDocument.AddressPersistentLocalId))),
                        new FieldSort { Order = SortOrder.Asc })
                });

                if (!string.IsNullOrEmpty(streetNameId)
                    || !string.IsNullOrEmpty(streetName)
                    || !string.IsNullOrEmpty(homonymAddition)
                    || !string.IsNullOrEmpty(houseNumber)
                    || !string.IsNullOrEmpty(boxNumber)
                    || !string.IsNullOrEmpty(postalCode)
                    || !string.IsNullOrEmpty(nisCode)
                    || !string.IsNullOrEmpty(municipalityName)
                    || !string.IsNullOrEmpty(status))
                {
                    descriptor.Query(query =>
                    {
                        query.Bool(b =>
                        {
                            var filterConditions = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                            if (!string.IsNullOrEmpty(streetNameId))
                                filterConditions.Add(m => m
                                    .Term(t => t
                                        .Field(
                                            $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.StreetNamePersistentLocalId))}"
                                            !)
                                        .Value(streetNameId)));

                            if (!string.IsNullOrEmpty(streetName))
                            {
                                var streetNameNames = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.Names))}";
                                filterConditions.Add(m => m.Nested(t => t.Path(streetNameNames!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(streetName)))));
                            }

                            if (!string.IsNullOrEmpty(homonymAddition))
                            {
                                var streetNameHomonymAdditions = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.HomonymAdditions))}";
                                filterConditions.Add(m => m.Nested(t => t.Path(streetNameHomonymAdditions!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameHomonymAdditions}.{NameSpelling}.{Keyword}"!)
                                        .Value(homonymAddition)))));
                            }

                            if (!string.IsNullOrEmpty(houseNumber))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.HouseNumber))}"!)
                                    .Value(houseNumber)));

                            if (!string.IsNullOrEmpty(boxNumber))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.BoxNumber))}"!)
                                    .Value(boxNumber)));

                            if (!string.IsNullOrEmpty(postalCode))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressSearchDocument.PostalInfo))}.{ToCamelCase(nameof(AddressSearchDocument.PostalInfo.PostalCode))}"
                                        !)
                                    .Value(postalCode)));

                            if (!string.IsNullOrEmpty(nisCode))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.NisCode))}"
                                        !)
                                    .Value(nisCode)));

                            if (!string.IsNullOrEmpty(municipalityName))
                            {
                                var municipalityNames = $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.Names))}";
                                filterConditions.Add(m => m.Nested(t => t.Path($"{municipalityNames}"!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(municipalityName)))));
                            }

                            if (!string.IsNullOrEmpty(status))
                            {
                                var addressStatus = ((AdresStatus)parsedStatus!).ConvertFromAdresStatus();
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.Status))}"!)
                                    .Value(Enum.GetName(addressStatus)!)));
                            }

                            b.Filter(filterConditions.ToArray());
                        });
                    });
                }
            });

            if (!searchResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to search for addresses", searchResponse.ElasticsearchServerError, searchResponse.DebugInformation);
            }

            return new AddressSearchResult(searchResponse.Documents, searchResponse.Total);
        }
    }
}
