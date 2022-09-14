namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectAddressPostalCode : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("b2d72770-bde5-44db-b1b4-dfd88ff9b323");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public PostalCode PostalCode { get; }
        public MunicipalityId PostalCodeMunicipalityId { get; set; }
        public Provenance Provenance { get; }

        public CorrectAddressPostalCode(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            PostalCode = postalCode;
            PostalCodeMunicipalityId = postalCodeMunicipalityId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectAddressPostalCode-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return PostalCode;
            yield return PostalCodeMunicipalityId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
