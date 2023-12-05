namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Address.Crab;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportSubaddressPositionFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("3ce00426-6093-4845-bd56-5c1df125be0c");

        public CrabAddressPositionId AddressPositionId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public WkbGeometry AddressPosition { get; }
        public CrabAddressNature AddressNature { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }

        public ImportSubaddressPositionFromCrab(
            CrabAddressPositionId addressPositionId,
            CrabSubaddressId subaddressId,
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
            SubaddressId = subaddressId;
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
            => Deterministic.Create(Namespace, $"ImportSubaddressPositionFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressPositionId;
            yield return SubaddressId;
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
