namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ProposeAddressForMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("75540a40-3528-4ddd-b716-6bc997911258");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public PostalCode PostalCode { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public HouseNumber HouseNumber { get; }
        public BoxNumber? BoxNumber { get; }
        public GeometryMethod GeometryMethod { get; }
        public GeometrySpecification GeometrySpecification { get; }
        public ExtendedWkbGeometry Position { get; }
        public bool OfficiallyAssigned { get; }

        public AddressPersistentLocalId MergedAddressPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public ProposeAddressForMunicipalityMerger(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position,
            bool officiallyAssigned,
            AddressPersistentLocalId mergedAddressPersistentLocalId,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            AddressPersistentLocalId = addressPersistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Position = position;
            OfficiallyAssigned = officiallyAssigned;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeAddressForMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return PostalCode;
            yield return HouseNumber;
            yield return BoxNumber ?? string.Empty;
            yield return GeometryMethod.ToString();
            yield return GeometrySpecification.ToString();
            yield return Position;
            yield return OfficiallyAssigned;
            yield return MergedAddressPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
