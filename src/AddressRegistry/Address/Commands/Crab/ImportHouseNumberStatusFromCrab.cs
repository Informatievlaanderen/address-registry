namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using ValueObjects.Crab;

    public class ImportHouseNumberStatusFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("573f7b70-a413-452b-a73d-a8839d9d02cd");

        public CrabHouseNumberStatusId HouseNumberStatusId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public CrabAddressStatus AddressStatus { get; }

        public ImportHouseNumberStatusFromCrab(
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberStatusId = houseNumberStatusId;
            HouseNumberId = houseNumberId;
            AddressStatus = addressStatus;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportHouseNumberStatusFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberStatusId;
            yield return HouseNumberId;
            yield return AddressStatus;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
