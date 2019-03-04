namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class AssignOsloIdForCrabHouseNumberId
    {
        private static readonly Guid Namespace = new Guid("8a3f242c-0166-4f86-9b66-9aa3be98c9ba");

        public CrabHouseNumberId HouseNumberId { get; }
        public OsloId OsloId { get; }
        public OsloAssignmentDate AssignmentDate { get; }

        public AssignOsloIdForCrabHouseNumberId(
            CrabHouseNumberId houseNumberId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            HouseNumberId = houseNumberId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignOsloId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
            yield return OsloId;
            yield return AssignmentDate;
        }
    }
}
