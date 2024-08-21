namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(IOptions<ResponseOptions> responseOptions)
        {
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressSearchResponse> Handle(AddressSearchRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Filtering.Filter.Query))
                return new AddressSearchResponse(new List<AddressSearchItem>());

            return new AddressSearchResponse(new List<AddressSearchItem>());
        }
    }
}
