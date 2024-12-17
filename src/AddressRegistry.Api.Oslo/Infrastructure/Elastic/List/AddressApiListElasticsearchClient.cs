namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.List
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
    using Projections.Elastic.AddressList;

    public sealed class AddressApiListElasticsearchClient: AddressApiElasticsearchClientBase, IAddressApiListElasticsearchClient
    {
        public AddressApiListElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias)
            : base(elasticsearchClient, indexAlias)
        { }

        public async Task<AddressListResult> ListAddresses(
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
            object? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && !Enum.TryParse(typeof(AdresStatus), status, true, out parsedStatus))
            {
                return new AddressListResult(Enumerable.Empty<AddressListDocument>().ToList(), 0);
            }

            var searchResponse = await ElasticsearchClient.SearchAsync<AddressListDocument>(IndexAlias, descriptor =>
            {
                descriptor.Size(size);
                descriptor.From(from);
                descriptor.TrackTotalHits(new TrackHits(true));
                descriptor.Sort(new List<SortOptions>
                {
                    SortOptions.Field(new Field(ToCamelCase(nameof(AddressListDocument.AddressPersistentLocalId))),
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
                            var filterConditions = new List<Action<QueryDescriptor<AddressListDocument>>>();

                            if (!string.IsNullOrEmpty(streetNameId))
                                filterConditions.Add(m => m
                                    .Term(t => t
                                        .Field(
                                            $"{ToCamelCase(nameof(AddressListDocument.StreetName))}.{ToCamelCase(nameof(AddressListDocument.StreetName.StreetNamePersistentLocalId))}"
                                            !)
                                        .Value(streetNameId)));

                            if (!string.IsNullOrEmpty(streetName))
                            {
                                var streetNameNames = $"{ToCamelCase(nameof(AddressListDocument.StreetName))}.{ToCamelCase(nameof(AddressListDocument.StreetName.Names))}";
                                filterConditions.Add(m => m.Nested(t => t.Path(streetNameNames!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(streetName)))));
                            }

                            if (!string.IsNullOrEmpty(homonymAddition))
                            {
                                var streetNameHomonymAdditions = $"{ToCamelCase(nameof(AddressListDocument.StreetName))}.{ToCamelCase(nameof(AddressListDocument.StreetName.HomonymAdditions))}";
                                filterConditions.Add(m => m.Nested(t => t.Path(streetNameHomonymAdditions!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameHomonymAdditions}.{NameSpelling}.{Keyword}"!)
                                        .Value(homonymAddition)))));
                            }

                            if (!string.IsNullOrEmpty(houseNumber))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressListDocument.HouseNumber))}"!)
                                    .Value(houseNumber)));

                            if (!string.IsNullOrEmpty(boxNumber))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressListDocument.BoxNumber))}"!)
                                    .Value(boxNumber)));

                            if (!string.IsNullOrEmpty(postalCode))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressListDocument.PostalInfo))}.{ToCamelCase(nameof(AddressListDocument.PostalInfo.PostalCode))}"
                                        !)
                                    .Value(postalCode)));

                            if (!string.IsNullOrEmpty(nisCode))
                                filterConditions.Add(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressListDocument.Municipality))}.{ToCamelCase(nameof(AddressListDocument.Municipality.NisCode))}"
                                        !)
                                    .Value(nisCode)));

                            if (!string.IsNullOrEmpty(municipalityName))
                            {
                                var municipalityNames = $"{ToCamelCase(nameof(AddressListDocument.Municipality))}.{ToCamelCase(nameof(AddressListDocument.Municipality.Names))}";
                                filterConditions.Add(m => m.Nested(t => t.Path($"{municipalityNames}"!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(municipalityName)))));
                            }

                            if (!string.IsNullOrEmpty(status))
                            {
                                var addressStatus = ((AdresStatus)parsedStatus!).ConvertFromAdresStatus();
                                filterConditions.Add(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressListDocument.Status))}"!)
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

            return new AddressListResult(searchResponse.Documents, searchResponse.Total);
        }
    }
}
