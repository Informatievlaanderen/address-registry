namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using StreetName;

    public interface IHasAddressPersistentLocalId
    {
        int AddressPersistentLocalId { get; }
    }
}
