namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectStreetNameRetirement : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("7c8deb3a-28b8-4ae6-a476-10404d585ba7");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public CorrectStreetNameRetirement(
            StreetNamePersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectStreetNameRetirement-{ToString()}");

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
