namespace AddressRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;

    public interface IEventsRepository
    {
        Task<int?> GetAddressPersistentLocalId(Guid addressId);
    }
}
