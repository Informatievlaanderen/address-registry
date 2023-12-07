namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RenameStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("7403446a-6422-4ce3-87fa-6b1a70e5196f");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public StreetNamePersistentLocalId DestinationPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public RenameStreetName(
            StreetNamePersistentLocalId persistentLocalId,
            StreetNamePersistentLocalId destinationPersistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            DestinationPersistentLocalId = destinationPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RenameStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;
            yield return DestinationPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
