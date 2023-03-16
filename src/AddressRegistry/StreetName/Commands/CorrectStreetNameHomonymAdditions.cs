namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectStreetNameHomonymAdditions : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("72387c66-1bc9-47b8-9ded-48774e06024e");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public IDictionary<string, string> HomonymAdditions { get; }

        public Provenance Provenance { get; }

        public CorrectStreetNameHomonymAdditions(
            StreetNamePersistentLocalId persistentLocalId,
            IDictionary<string, string> homonymAdditions,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            HomonymAdditions = homonymAdditions;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectStreetNameHomonymAdditions-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var homonymAddition in HomonymAdditions)
            {
                yield return $"{homonymAddition.Key}: {homonymAddition.Value}";
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
