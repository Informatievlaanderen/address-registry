namespace AddressRegistry.Api.Oslo.Address.V3.Detail
{
    using MediatR;

    public sealed record AddressDetailOsloRequest(int PersistentLocalId) : IRequest<AddressDetailOsloV3Response>;
}
