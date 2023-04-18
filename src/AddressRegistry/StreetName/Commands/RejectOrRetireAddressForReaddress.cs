namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RejectOrRetireAddressForReaddress : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("981bc7bf-bcfa-44b2-b105-0131d44a4d11");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public StreetNamePersistentLocalId DestinationStreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId  { get; }
        public AddressPersistentLocalId DestinationAddressPersistentLocalId  { get; }
        public IList<BoxNumberAddressPersistentLocalId> DestinationBoxNumbers  { get; }
        public Provenance Provenance { get; }

        public RejectOrRetireAddressForReaddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            StreetNamePersistentLocalId destinationStreetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            IList<BoxNumberAddressPersistentLocalId> destinationBoxNumbers, Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            DestinationStreetNamePersistentLocalId = destinationStreetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
            DestinationBoxNumbers = destinationBoxNumbers;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RejectOrRetireAddressForReaddress-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return DestinationStreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return DestinationAddressPersistentLocalId;

            foreach (var boxNumber in DestinationBoxNumbers)
            {
                yield return boxNumber.BoxNumber;
                yield return boxNumber.AddressPersistentLocalId;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }

    public record BoxNumberAddressPersistentLocalId(
        BoxNumber BoxNumber,
        AddressPersistentLocalId AddressPersistentLocalId);
}
