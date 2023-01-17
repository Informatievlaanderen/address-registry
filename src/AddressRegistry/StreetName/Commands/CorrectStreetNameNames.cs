namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectStreetNameNames : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("414054a9-6ca0-4b76-8036-2c00be4c1a59");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public IDictionary<string, string> StreetNameNames { get; }

        public Provenance Provenance { get; }

        public CorrectStreetNameNames(
            StreetNamePersistentLocalId persistentLocalId,
            IDictionary<string, string> streetNameNames,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            StreetNameNames = streetNameNames;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectStreetNameNames-{ToString()}");

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
