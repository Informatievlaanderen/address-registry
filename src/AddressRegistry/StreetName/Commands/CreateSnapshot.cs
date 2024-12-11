namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CreateSnapshot : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("b46ad9f6-ed80-48be-ba54-8c70abd21d34");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public Provenance Provenance { get; }

        public CreateSnapshot(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"CreateSnapshot-{ToString()}");

        public override string? ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
