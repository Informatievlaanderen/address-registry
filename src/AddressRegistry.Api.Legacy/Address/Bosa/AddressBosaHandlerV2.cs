namespace AddressRegistry.Api.Legacy.Address.Bosa
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public sealed class AddressBosaHandlerV2 : IRequestHandler<AddressBosaRequest, AddressBosaResponse>
    {
        private readonly AddressBosaContext _bosaContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressBosaHandlerV2(
            AddressBosaContext bosaContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _bosaContext = bosaContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressBosaResponse> Handle(AddressBosaRequest request, CancellationToken cancellationToken)
        {
            var query = new AddressBosaQueryV2(_bosaContext, _responseOptions.Value);
            return await query.Filter(request);
        }
    }
}
