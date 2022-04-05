namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using ValueObjects;

    public class AssignPersistentLocalIdForCrabHouseNumberId
    {
        private static readonly Guid Namespace = new Guid("8a3f242c-0166-4f86-9b66-9aa3be98c9ba");

        public CrabHouseNumberId HouseNumberId { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalIdAssignmentDate AssignmentDate { get; }

        public AssignPersistentLocalIdForCrabHouseNumberId(
            CrabHouseNumberId houseNumberId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            HouseNumberId = houseNumberId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignPersistentLocalId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
            yield return PersistentLocalId;
            yield return AssignmentDate;
        }
    }
}
