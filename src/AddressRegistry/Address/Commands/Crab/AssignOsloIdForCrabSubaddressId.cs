namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class AssignOsloIdForCrabSubaddressId
    {
        private static readonly Guid Namespace = new Guid("044b1a30-de76-4be0-88db-3711c26e7174");

        public CrabSubaddressId SubaddressId { get; }
        public OsloId OsloId { get; }
        public OsloAssignmentDate AssignmentDate { get; }

        public AssignOsloIdForCrabSubaddressId(
            CrabSubaddressId subaddressId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            SubaddressId = subaddressId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignOsloId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return SubaddressId;
            yield return OsloId;
            yield return AssignmentDate;
        }
    }
}
