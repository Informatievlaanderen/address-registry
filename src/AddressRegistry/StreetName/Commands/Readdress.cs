namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public record struct ReaddressAddressItem(
        StreetNamePersistentLocalId SourceStreetNamePersistentLocalId,
        AddressPersistentLocalId SourceAddressPersistentLocalId,
        HouseNumber DestinationHouseNumber);

    public record struct RetireAddressItem(
        StreetNamePersistentLocalId StreetNamePersistentLocalId,
        AddressPersistentLocalId AddressPersistentLocalId);

    public sealed class ReaddressExecutionContext
    {
        public List<(StreetNamePersistentLocalId streetNamePersistentLocalId, AddressPersistentLocalId addressPersistentLocalId)> AddressesAdded { get; }
        public List<(StreetNamePersistentLocalId streetNamePersistentLocalId, AddressPersistentLocalId addressPersistentLocalId)> AddressesUpdated { get; }

        public ReaddressExecutionContext()
        {
            AddressesAdded = new List<(StreetNamePersistentLocalId streetNamePersistentLocalId, AddressPersistentLocalId addressPersistentLocalId)>();
            AddressesUpdated = new List<(StreetNamePersistentLocalId streetNamePersistentLocalId, AddressPersistentLocalId addressPersistentLocalId)>();
        }
    }

    public sealed class Readdress : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("24e83ba0-e041-4098-96fc-6b64a1f82d43");

        public StreetNamePersistentLocalId DestinationStreetNamePersistentLocalId { get; }
        public List<ReaddressAddressItem> ReaddressAddressItems { get; }
        public List<RetireAddressItem> RetireAddressItems { get; }

        public Provenance Provenance { get; }

        public ReaddressExecutionContext ExecutionContext { get; }

        public Readdress(
            StreetNamePersistentLocalId destinationStreetNamePersistentLocalId,
            List<ReaddressAddressItem> readdressAddressItems,
            List<RetireAddressItem> retireAddressItems,
            Provenance provenance)
        {
            DestinationStreetNamePersistentLocalId = destinationStreetNamePersistentLocalId;
            ReaddressAddressItems = readdressAddressItems;
            RetireAddressItems = retireAddressItems;

            Provenance = provenance;

            ExecutionContext = new ReaddressExecutionContext();
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"Readdress-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return DestinationStreetNamePersistentLocalId;

            foreach (var item in ReaddressAddressItems)
            {
                yield return item.SourceStreetNamePersistentLocalId;
                yield return item.SourceAddressPersistentLocalId;
                yield return item.DestinationHouseNumber;
            }

            foreach (var item in RetireAddressItems)
            {
                yield return item.StreetNamePersistentLocalId;
                yield return item.AddressPersistentLocalId;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
