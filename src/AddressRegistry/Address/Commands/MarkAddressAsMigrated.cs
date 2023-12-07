namespace AddressRegistry.Address.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class MarkAddressAsMigrated : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("1ac91a48-f860-4fe1-a4cd-10bf2daf8421");

        public AddressId AddressId { get; }
        public StreetName.StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public Provenance Provenance { get; }

        public MarkAddressAsMigrated(
            AddressId addressId,
            StreetName.StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            AddressId = addressId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MarkAddressAsMigrated-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressId;
            yield return StreetNamePersistentLocalId;
        }
    }
}
