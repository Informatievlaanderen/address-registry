namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    /// <summary>
    /// This is the opposite of <see cref="AddressWasRegularized" />.
    /// We realize that it is not the opposite in meaning. When deregulated was chosen, the translation engine(s) translated the dutch `Deregulariseren` to deregulate.
    /// This is because the Dutch word is not known in dictionaries either. We wanted to change this to `Deregularized` since it's more consistent and less wrong.
    /// We weren't able to push the change through.
    ///
    /// If you have lost time finding the opposite meaning or being confused, add your (optional) name and time lost :).
    /// - xxx : 1 min
    ///
    /// If this comment helped you add a +1!
    /// </summary>
    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("Het adres kreeg aanduiding 'niet officieel toegekend'. Wanneer de status van het adres voorgesteld is, zal de status wijzigen naar 'inGebruik'.")]
    public class AddressWasDeregulated : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressWasDeregulated"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasDeregulated(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }

        [JsonConstructor]
        private AddressWasDeregulated(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
