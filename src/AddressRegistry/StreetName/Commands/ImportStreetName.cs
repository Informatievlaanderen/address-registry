namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("10e50b34-94ad-435d-98b9-52bee77c8d99");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public MunicipalityId MunicipalityId { get; }
        public StreetNameStatus StreetNameStatus { get; }
        public Provenance Provenance { get; }

        public ImportStreetName(
            StreetNamePersistentLocalId persistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            MunicipalityId = municipalityId;
            StreetNameStatus = streetNameStatus;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;
            yield return MunicipalityId;
            yield return StreetNameStatus;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
