namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ProposeAddressesForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("75540a40-3528-4ddd-b716-6bc997911258");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public List<ProposeAddressesForMunicipalityMergerItem> Addresses { get; }

        public Provenance Provenance { get; }

        public ProposeAddressesForMunicipalityMerger(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            IEnumerable<ProposeAddressesForMunicipalityMergerItem> addresses,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Addresses = addresses.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeAddressesForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;

            foreach (var address in Addresses)
            {
                foreach (var field in address.GetIdentityFields())
                {
                    yield return field;
                }
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }

    public class ProposeAddressesForMunicipalityMergerItem
    {
        public PostalCode PostalCode { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public HouseNumber HouseNumber { get; }
        public BoxNumber? BoxNumber { get; }
        public GeometryMethod GeometryMethod { get; }
        public GeometrySpecification GeometrySpecification { get; }
        public ExtendedWkbGeometry Position { get; }
        public bool OfficiallyAssigned { get; }

        public StreetNamePersistentLocalId MergedStreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId MergedAddressPersistentLocalId { get; }

        public ProposeAddressesForMunicipalityMergerItem(
            PostalCode postalCode,
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position,
            bool officiallyAssigned,
            StreetNamePersistentLocalId mergedStreetNamePersistentLocalId,
            AddressPersistentLocalId mergedAddressPersistentLocalId)
        {
            PostalCode = postalCode;
            AddressPersistentLocalId = addressPersistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Position = position;
            OfficiallyAssigned = officiallyAssigned;
            MergedStreetNamePersistentLocalId = mergedStreetNamePersistentLocalId;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId;
        }

        public IEnumerable<object> GetIdentityFields()
        {
            yield return AddressPersistentLocalId;
            yield return PostalCode;
            yield return HouseNumber;
            yield return BoxNumber ?? string.Empty;
            yield return GeometryMethod.ToString();
            yield return GeometrySpecification.ToString();
            yield return Position;
            yield return OfficiallyAssigned;
            yield return MergedStreetNamePersistentLocalId;
            yield return MergedAddressPersistentLocalId;
        }
    }
}
