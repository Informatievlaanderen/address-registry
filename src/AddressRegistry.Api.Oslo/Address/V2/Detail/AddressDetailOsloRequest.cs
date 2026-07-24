namespace AddressRegistry.Api.Oslo.Address.V2.Detail
{
    using MediatR;

    public sealed record AddressDetailOsloRequest(int PersistentLocalId) : IRequest<AddressDetailOsloResponse>;
}
