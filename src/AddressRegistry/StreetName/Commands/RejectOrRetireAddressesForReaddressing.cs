namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RejectOrRetireAddressesForReaddressing : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("981bc7bf-bcfa-44b2-b105-0131d44a4d11");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public IList<AddressPersistentLocalId> AddressPersistentLocalIds { get; }
        public Provenance Provenance { get; }

        public RejectOrRetireAddressesForReaddressing(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IList<AddressPersistentLocalId> addressPersistentLocalIds,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalIds = addressPersistentLocalIds;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RejectOrRetireAddressesForReaddressing-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;

            foreach (var addressPersistentLocalId in AddressPersistentLocalIds)
            {
                yield return addressPersistentLocalId;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
