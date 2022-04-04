namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RemoveStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("a9b54170-1aa9-4770-a215-bea0ffd207fc");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public RemoveStreetName(
            StreetNamePersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RemoveStreetName-{ToString()}");

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
