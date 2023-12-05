namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Address.Crab;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportSubaddressMailCantonFromCrab : BaseCrabCommand
    {
        private static readonly Guid Namespace = new Guid("c917db9e-5d35-4c62-8f11-e33ea8b52b78");

        public CrabHouseNumberMailCantonId HouseNumberMailCantonId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public CrabSubaddressId SubaddressId { get; }
        public CrabMailCantonId MailCantonId { get; }
        public CrabMailCantonCode MailCantonCode { get; }

        public ImportSubaddressMailCantonFromCrab(
            CrabHouseNumberMailCantonId houseNumberMailCantonId,
            CrabHouseNumberId houseNumberId,
            CrabSubaddressId subaddressId,
            CrabMailCantonId mailCantonId,
            CrabMailCantonCode mailCantonCode,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberMailCantonId = houseNumberMailCantonId;
            HouseNumberId = houseNumberId;
            SubaddressId = subaddressId;
            MailCantonId = mailCantonId;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
            MailCantonCode = mailCantonCode;
        }

        public override Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressMailCantonFromCrab-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberMailCantonId;
            yield return HouseNumberId;
            yield return SubaddressId;
            yield return MailCantonId;
            yield return MailCantonCode;
            yield return Lifetime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
