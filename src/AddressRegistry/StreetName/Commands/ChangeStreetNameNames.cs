namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ChangeStreetNameNames : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("8f972fba-025d-43f3-9812-e13ddbd464d6");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public IDictionary<string, string> StreetNameNames { get; }

        public Provenance Provenance { get; }

        public ChangeStreetNameNames(
            StreetNamePersistentLocalId persistentLocalId,
            IDictionary<string, string> streetNameNames,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            StreetNameNames = streetNameNames;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeStreetNameNames-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var streetNameName in StreetNameNames)
            {
                yield return $"{streetNameName.Key}: {streetNameName.Value}";
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
