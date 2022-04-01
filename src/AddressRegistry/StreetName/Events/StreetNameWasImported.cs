namespace AddressRegistry.StreetName.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("De straatnaam werd geimporteerd.")]
    public class StreetNameWasImported : IStreetNameEvent
    {
        public const string EventName = "StreetNameWasImported"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public Guid MunicipalityId { get; }
        public StreetNameStatus StreetNameStatus { get; }
        public ProvenanceData Provenance { get; private set; }

        public StreetNameWasImported(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            MunicipalityId = municipalityId;
            StreetNameStatus = streetNameStatus;
        }

        [JsonConstructor]
        private StreetNameWasImported(
            int streetNamePersistentLocalId,
            Guid municipalityId,
            StreetNameStatus streetNameStatus,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new MunicipalityId(municipalityId),
                streetNameStatus)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => ""; //this.ToHash(EventName);//TODO: when package pushed to nuget
    }
}

