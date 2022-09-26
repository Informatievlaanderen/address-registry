namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectAddressBoxNumber : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("ddb0e558-d1e3-4fb6-9eda-b92b58820a68");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public BoxNumber BoxNumber { get; }
        public Provenance Provenance { get; }

        public CorrectAddressBoxNumber(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            BoxNumber boxNumber,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            BoxNumber = boxNumber;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectAddressBoxNumber-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return BoxNumber;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
