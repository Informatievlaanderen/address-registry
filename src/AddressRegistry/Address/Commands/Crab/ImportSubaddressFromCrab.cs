namespace AddressRegistry.Address.Commands.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class ImportSubaddressFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("dee3cd1e-93d7-42dd-9366-aa0d026fb0e4");

        public CrabSubaddressId SubaddressId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public BoxNumber BoxNumber { get; }
        public CrabBoxNumberType BoxNumberType { get; }

        public ImportSubaddressFromCrab(
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            SubaddressId = subaddressId;
            HouseNumberId = houseNumberId;
            BoxNumber = boxNumber;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
            BoxNumberType = boxNumberType;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return SubaddressId;
            yield return HouseNumberId;
            yield return BoxNumber;
            yield return BoxNumberType;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
