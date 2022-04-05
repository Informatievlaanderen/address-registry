namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using ValueObjects;
    using ValueObjects.Crab;

    public class ImportHouseNumberFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("47ebb853-2da6-4702-b6ca-643449e406a5");

        public CrabHouseNumberId HouseNumberId { get; }
        public CrabStreetNameId StreetNameId { get; }
        public HouseNumber HouseNumber { get; }
        public GrbNotation GrbNotation { get; }

        public ImportHouseNumberFromCrab(
            CrabHouseNumberId houseNumberId,
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
            => Deterministic.Create(Namespace, $"ImportHouseNumberFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
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
