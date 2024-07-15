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

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; set; }
        public PostalCode PostalCode { get; set; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; set; }
        public HouseNumber HouseNumber { get; set; }
        public BoxNumber? BoxNumber { get; set; }
        public GeometryMethod GeometryMethod { get; set; }
        public GeometrySpecification GeometrySpecification { get; set; }
        public ExtendedWkbGeometry Position { get; set; }
        public bool OfficiallyAssigned { get; }
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

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
