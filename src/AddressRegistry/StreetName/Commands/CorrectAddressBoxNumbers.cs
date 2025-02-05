namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CorrectAddressBoxNumbers : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("3c1abce4-7f6d-4136-b955-4c3a33d8cfed");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public Dictionary<AddressPersistentLocalId, BoxNumber> AddressBoxNumbers { get; }
        public Provenance Provenance { get; }

        public CorrectAddressBoxNumbers(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Dictionary<AddressPersistentLocalId, BoxNumber> addressBoxNumbers,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressBoxNumbers = addressBoxNumbers;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectAddressBoxNumbers-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            foreach (var x in AddressBoxNumbers)
            {
                yield return x.Key;
                yield return x.Value;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
