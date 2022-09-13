namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ChangeAddressPostalCode : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("a80ac14f-de86-4061-8411-856c9d70f7bc");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public PostalCode PostalCode { get; }
        public Provenance Provenance { get; }

        public ChangeAddressPostalCode(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            PostalCode = postalCode;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeAddressPostalCode-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return PostalCode;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
