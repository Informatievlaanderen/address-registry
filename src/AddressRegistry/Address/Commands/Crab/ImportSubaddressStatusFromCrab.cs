namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Address.Crab;

    public class ImportSubaddressStatusFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("219b9413-8a8b-462c-9e14-3c84047afd62");

        public CrabSubaddressStatusId SubaddressStatusId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public CrabAddressStatus AddressStatus { get; }

        public ImportSubaddressStatusFromCrab(
            CrabSubaddressStatusId subaddressStatusId,
            CrabSubaddressId subaddressId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            SubaddressStatusId = subaddressStatusId;
            SubaddressId = subaddressId;
            AddressStatus = addressStatus;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressStatusFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return SubaddressStatusId;
            yield return SubaddressId;
            yield return AddressStatus;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
