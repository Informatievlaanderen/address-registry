namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RetireAddressesBecauseStreetNameWasRenamed : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("cb9535c1-bcec-47b3-87fe-038f375d878a");

        public StreetNamePersistentLocalId PersistentLocalId { get; }

        public List<AddressPersistentLocalId> AddressesToRetire { get; }
        public Provenance Provenance { get; }

        public RetireAddressesBecauseStreetNameWasRenamed(
            StreetNamePersistentLocalId persistentLocalId,
            List<AddressPersistentLocalId> addressPersistentLocalIds,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            AddressesToRetire = addressPersistentLocalIds;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireAddressesBecauseStreetNameWasRenamed-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;
            yield return AddressesToRetire;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
