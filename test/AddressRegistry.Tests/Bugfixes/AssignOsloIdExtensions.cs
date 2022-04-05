namespace AddressRegistry.Tests.Bugfixes
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;

    public static class AssignPersistentLocalIdForCrabSubaddressIdExtensions
    {
        public static AddressPersistentLocalIdWasAssigned ToLegacyEvent(this AssignPersistentLocalIdForCrabSubaddressId command)
        {
            return new AddressPersistentLocalIdWasAssigned(new AddressId(command.SubaddressId.CreateDeterministicId()), command.PersistentLocalId, command.AssignmentDate);
        }
    }
}
