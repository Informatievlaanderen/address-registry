namespace AddressRegistry.Address.Commands
{
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using ValueObjects;

    public class AssignPersistentLocalIdToAddress
    {
        private static readonly Guid Namespace = new Guid("213d9961-c1fc-4e3a-b512-29d0221b34ab");

        public AddressId AddressId { get; }
        public PersistentLocalId PersistentLocalId { get; }

        public AssignPersistentLocalIdToAddress(
            AddressId addressId,
            PersistentLocalId persistentLocalId)
        {
            AddressId = addressId;
            PersistentLocalId = persistentLocalId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"AssignPersistentLocalIdToAddress-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressId;
            yield return PersistentLocalId;
        }
    }
}
