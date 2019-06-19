namespace AddressRegistry.Address.Commands.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class ImportHouseNumberSubaddressFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("29e91891-8e7e-4e22-91ab-3608f6802b2b");

        public CrabHouseNumberId HouseNumberId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public CrabStreetNameId StreetNameId { get; }
        public HouseNumber HouseNumber { get; }
        public GrbNotation GrbNotation { get; }

        public ImportHouseNumberSubaddressFromCrab(
            CrabHouseNumberId houseNumberId,
            CrabSubaddressId subaddressId,
            CrabStreetNameId streetNameId,
            HouseNumber houseNumber,
            GrbNotation grbNotation,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberId = houseNumberId;
            SubaddressId = subaddressId;
            StreetNameId = streetNameId;
            HouseNumber = houseNumber;
            GrbNotation = grbNotation;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportHouseNumberSubaddressFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
            yield return SubaddressId;
            yield return StreetNameId;
            yield return HouseNumber;
            yield return GrbNotation;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
