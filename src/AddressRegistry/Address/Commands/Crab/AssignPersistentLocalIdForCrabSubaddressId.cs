namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class AssignPersistentLocalIdForCrabSubaddressId
    {
        private static readonly Guid Namespace = new Guid("044b1a30-de76-4be0-88db-3711c26e7174");

        public CrabSubaddressId SubaddressId { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalIdAssignmentDate AssignmentDate { get; }

        public AssignPersistentLocalIdForCrabSubaddressId(
            CrabSubaddressId subaddressId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            SubaddressId = subaddressId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignPersistentLocalId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return SubaddressId;
            yield return PersistentLocalId;
            yield return AssignmentDate;
        }
    }
}
