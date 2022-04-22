namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    
    public class ImportMigratedStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("10e50b34-94ad-435d-98b9-52bee77c8d99");

        public StreetNameId StreetNameId { get; }
        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public MunicipalityId MunicipalityId { get; }
        public NisCode NisCode { get; }
        public StreetNameStatus StreetNameStatus { get; }
        public Provenance Provenance { get; }

        public ImportMigratedStreetName(
            StreetNameId streetNameId,
            StreetNamePersistentLocalId persistentLocalId,
            MunicipalityId municipalityId,
            NisCode nisCode,
            StreetNameStatus streetNameStatus,
            Provenance provenance)
        {
            StreetNameId = streetNameId;
            PersistentLocalId = persistentLocalId;
            MunicipalityId = municipalityId;
            NisCode = nisCode;
            StreetNameStatus = streetNameStatus;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNameId;
            yield return PersistentLocalId;
            yield return MunicipalityId;
            yield return StreetNameStatus;
            yield return NisCode;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
