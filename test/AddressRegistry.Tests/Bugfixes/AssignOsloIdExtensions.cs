namespace AddressRegistry.Tests.Bugfixes
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;

    public static class AssignPersistentLocalIdForCrabSubaddressIdExtensions
    {
        public static AddressPersistentLocalIdWasAssigned ToLegacyEvent(this AssignPersistentLocalIdForCrabSubaddressId command)
        {
            return new AddressPersistentLocalIdWasAssigned(new AddressId(command.SubaddressId.CreateDeterministicId()), command.PersistentLocalId, command.AssignmentDate);
        }
    }
}
