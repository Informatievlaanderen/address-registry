namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Address.Crab;

    public class ImportHouseNumberPositionFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("3c920a6f-516c-4ea7-b347-80182125ccd2");

        public CrabAddressPositionId AddressPositionId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public WkbGeometry AddressPosition { get; }
        public CrabAddressNature AddressNature { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }

        public ImportHouseNumberPositionFromCrab(
            CrabAddressPositionId addressPositionId,
            CrabHouseNumberId houseNumberId,
            WkbGeometry addressPosition,
            CrabAddressNature addressNature,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            AddressPositionId = addressPositionId;
            HouseNumberId = houseNumberId;
            AddressPosition = addressPosition;
            AddressNature = addressNature;
            AddressPositionOrigin = addressPositionOrigin;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportHouseNumberPositionFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressPositionId;
            yield return HouseNumberId;
            yield return AddressPosition;
            yield return AddressNature;
            yield return AddressPositionOrigin;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
