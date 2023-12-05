namespace AddressRegistry.Api.Legacy.Address.Detail
{
    using MediatR;

    public sealed record AddressDetailRequest(int PersistentLocalId) : IRequest<AddressDetailResponse>;
}
