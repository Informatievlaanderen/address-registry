namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RetireStreetNameBecauseOfMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("4c302c00-8e15-41fd-871f-3122dc837fd9");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public IReadOnlyList<StreetNamePersistentLocalId> NewPersistentLocalIds { get; }
        public IDictionary<AddressPersistentLocalId, AddressPersistentLocalId> NewAddressPersistentLocalIdsByMerged { get; }
        public Provenance Provenance { get; }

        public RetireStreetNameBecauseOfMunicipalityMerger(
            StreetNamePersistentLocalId persistentLocalId,
            IEnumerable<StreetNamePersistentLocalId> newPersistentLocalIds,
            IDictionary<AddressPersistentLocalId, AddressPersistentLocalId> newAddressPersistentLocalIdsByMerged,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            NewPersistentLocalIds = newPersistentLocalIds.ToList();
            NewAddressPersistentLocalIdsByMerged = newAddressPersistentLocalIdsByMerged;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireStreetNameBecauseOfMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var newPersistentLocalId in NewPersistentLocalIds)
            {
                yield return newPersistentLocalId;
            }

            foreach (var mergedAddressPersistentLocalId in NewAddressPersistentLocalIdsByMerged.Select(x => x.Key))
            {
                yield return mergedAddressPersistentLocalId;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
