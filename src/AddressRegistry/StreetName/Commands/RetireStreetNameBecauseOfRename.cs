namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RetireStreetNameBecauseOfRename : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("32e8aabd-67fb-4b59-9071-de2b226f701b");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public RetireStreetNameBecauseOfRename(
            StreetNamePersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireStreetNameBecauseOfRename-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
