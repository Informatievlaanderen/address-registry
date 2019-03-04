namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;

    public class RequestOsloIdForCrabHouseNumberId
    {
        private static readonly Guid Namespace = new Guid("e07178dc-249a-425b-84a5-d065af8a84cc");

        public CrabHouseNumberId HouseNumberId { get; }

        public RequestOsloIdForCrabHouseNumberId(
            CrabHouseNumberId houseNumberId)
        {
            HouseNumberId = houseNumberId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"RequestOsloId-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return HouseNumberId;
        }
    }
}
