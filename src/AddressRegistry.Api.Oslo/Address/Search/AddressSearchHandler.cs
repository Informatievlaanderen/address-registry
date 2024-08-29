namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure.Elastic;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly IAddressApiElasticsearchClient _addressApiElasticsearchClient;
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(
            IAddressApiElasticsearchClient addressApiElasticsearchClient,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressApiElasticsearchClient = addressApiElasticsearchClient;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressSearchResponse> Handle(AddressSearchRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Filtering.Filter.Query))
                return new AddressSearchResponse(new List<AddressSearchItem>());

            var query = request.Filtering.Filter.Query;

            if (ContainsNumberAfterSpace(query))
            {
                return new AddressSearchResponse(new List<AddressSearchItem>());
            }

            var streetNames = query.Split(' ');
            streetNames = streetNames.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var streetNameResult = new List<StreetNameSearchResult>();
            if (streetNames.Length > 1)
            {
                var municipalityOrPostalName = streetNames.Last();
                streetNames = streetNames.Take(streetNames.Length - 1).ToArray();
                streetNameResult = (await _addressApiElasticsearchClient.SearchStreetNames([string.Join(' ', streetNames), municipalityOrPostalName], municipalityOrPostalName)).ToList();
            }
            else
            {
                streetNameResult = (await _addressApiElasticsearchClient.SearchStreetNames(request.Filtering.Filter.Query)).ToList();
            }

            return new AddressSearchResponse(streetNameResult
                .Select(x => new AddressSearchItem(
                    $"{_responseOptions.StraatNaamNaamruimte.Trim('/')}/{x.StreetNamePersistentLocalId}",
                    x.StreetNamePersistentLocalId.ToString(),
                    new Uri(string.Format(_responseOptions.StraatnaamDetailUrl, x.StreetNamePersistentLocalId)),
                    x.Spelling)));
        }

        private static bool ContainsNumberAfterSpace(string input)
        {
            return Regex.IsMatch(input, @"\s\d\w*");
        }
    }
}
