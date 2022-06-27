namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ProposeAddress : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("33efb29a-d7dc-4557-9798-04d3b3cd76f5");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; set; }
        public PostalCode PostalCode { get; set; }
        public MunicipalityId PostalCodeMunicipalityId { get; set; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; set; }
        public HouseNumber HouseNumber { get; set; }
        public BoxNumber? BoxNumber { get; set; }
        public Provenance Provenance { get; }
        
        public ProposeAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId,
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            PostalCodeMunicipalityId = postalCodeMunicipalityId;
            AddressPersistentLocalId = addressPersistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ProposeAddress-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return PostalCodeMunicipalityId;
            yield return PostalCode;
            yield return AddressPersistentLocalId;
            yield return HouseNumber;
            yield return BoxNumber ?? string.Empty;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
