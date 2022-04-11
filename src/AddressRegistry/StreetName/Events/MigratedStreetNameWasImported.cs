namespace AddressRegistry.StreetName.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("De gemigreerde straatnaam werd geimporteerd.")]
    public class MigratedStreetNameWasImported : IStreetNameEvent
    {
        public const string EventName = "MigratedStreetNameWasImported"; // BE CAREFUL CHANGING THIS!!

        public Guid StreetNameId { get; }
        public int StreetNamePersistentLocalId { get; }
        public Guid MunicipalityId { get; }
        public StreetNameStatus StreetNameStatus { get; }
        public ProvenanceData Provenance { get; private set; }

        public MigratedStreetNameWasImported(
            StreetNameId streetNameId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus)
        {
            StreetNameId = streetNameId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            MunicipalityId = municipalityId;
            StreetNameStatus = streetNameStatus;
        }

        [JsonConstructor]
        private MigratedStreetNameWasImported(
            Guid streetNameId,
            int streetNamePersistentLocalId,
            Guid municipalityId,
            StreetNameStatus streetNameStatus,
            ProvenanceData provenance)
            : this(
                new StreetNameId(streetNameId),
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new MunicipalityId(municipalityId),
                streetNameStatus)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(MunicipalityId.ToString("D"));
            fields.Add(StreetNameStatus.ToString());
            fields.Add(StreetNameId.ToString("D"));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
