namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class RequestPersistentLocalIdForCrabHouseNumberId
    {
        private static readonly Guid Namespace = new Guid("e07178dc-249a-425b-84a5-d065af8a84cc");

        public CrabHouseNumberId HouseNumberId { get; }

        public RequestPersistentLocalIdForCrabHouseNumberId(
            CrabHouseNumberId houseNumberId)
        {
            HouseNumberId = houseNumberId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"RequestPersistentLocalIdForCrabHouseNumberId-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
        }
    }
}
