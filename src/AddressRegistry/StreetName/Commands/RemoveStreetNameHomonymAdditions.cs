namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RemoveStreetNameHomonymAdditions : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("b7b54611-e8cc-4116-8805-cdaaabbcf968");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public IList<string> Languages { get; }

        public Provenance Provenance { get; }

        public RemoveStreetNameHomonymAdditions(
            StreetNamePersistentLocalId persistentLocalId,
            IList<string> languages,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Languages = languages;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RemoveStreetNameHomonymAdditions-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var language in Languages)
            {
                yield return language;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
