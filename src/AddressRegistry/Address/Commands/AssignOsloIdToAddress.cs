namespace AddressRegistry.Address.Commands
{
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class AssignOsloIdToAddress
    {
        private static readonly Guid Namespace = new Guid("213d9961-c1fc-4e3a-b512-29d0221b34ab");

        public AddressId AddressId { get; }
        public OsloId OsloId { get; }

        public AssignOsloIdToAddress(
            AddressId addressId,
            OsloId osloId)
        {
            AddressId = addressId;
            OsloId = osloId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"AssignOsloIdToAddress-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressId;
            yield return OsloId;
        }
    }
}
