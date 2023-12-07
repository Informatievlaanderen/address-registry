namespace AddressRegistry.Api.Oslo.Address.Detail
{
    using MediatR;

    public sealed record AddressDetailOsloRequest(int PersistentLocalId) : IRequest<AddressDetailOsloResponse>;
}
