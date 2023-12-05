namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class RequestPersistentLocalIdForCrabSubaddressId
    {
        private static readonly Guid Namespace = new Guid("652158db-2e78-423b-8b46-df0eceb9bd65");

        public CrabSubaddressId SubaddressId { get; }

        public RequestPersistentLocalIdForCrabSubaddressId(
            CrabSubaddressId subaddressId)
        {
            SubaddressId = subaddressId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"RequestPersistentLocalIdForCrabSubaddressId-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return SubaddressId;
        }
    }
}
