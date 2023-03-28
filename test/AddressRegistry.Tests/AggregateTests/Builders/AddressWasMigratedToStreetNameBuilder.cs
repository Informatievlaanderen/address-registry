namespace AddressRegistry.Tests.AggregateTests.Builders
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Events;

    public class AddressWasMigratedToStreetNameBuilder
    {
        private readonly Fixture _fixture;

        private readonly AddressStatus _addressStatus;

        private StreetNamePersistentLocalId? _streetNamePersistentLocalId;
        private AddressPersistentLocalId? _addressPersistentLocalId;
        private AddressPersistentLocalId? _parentAddressPersistentLocalId;
        private HouseNumber? _houseNumber;
        private BoxNumber? _boxNumber;
        private PostalCode? _postalCode;
        private AddressGeometry? _addressGeometry;
        private bool _isRemoved;

        public AddressWasMigratedToStreetNameBuilder(Fixture fixture, AddressStatus addressStatus = AddressStatus.Proposed)
        {
            _fixture = fixture;
            _addressStatus = addressStatus;
            _postalCode = _fixture.Create<PostalCode>();
        }

        public AddressWasMigratedToStreetNameBuilder WithStreetNamePersistentLocalId(StreetNamePersistentLocalId persistentLocalId)
        {
            _streetNamePersistentLocalId = persistentLocalId;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithAddressPersistentLocalId(AddressPersistentLocalId persistentLocalId)
        {
            _addressPersistentLocalId = persistentLocalId;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithHouseNumber(HouseNumber houseNumber)
        {
            _houseNumber = houseNumber;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithBoxNumber(BoxNumber boxNumber, AddressPersistentLocalId parentAddressPersistentLocalId)
        {
            _boxNumber = boxNumber;
            _parentAddressPersistentLocalId = parentAddressPersistentLocalId;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithIsRemoved(bool isRemoved = true)
        {
            _isRemoved = isRemoved;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithPostalCode(PostalCode? postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public AddressWasMigratedToStreetNameBuilder WithAddressGeometry(AddressGeometry addressGeometry)
        {
            _addressGeometry = addressGeometry;
            return this;
        }

        public AddressWasMigratedToStreetName Build()
        {
            var @event = new AddressWasMigratedToStreetName(
                _streetNamePersistentLocalId ?? _fixture.Create<StreetNamePersistentLocalId>(),
                _fixture.Create<AddressId>(),
                _fixture.Create<AddressStreetNameId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _addressStatus,
                _houseNumber ?? _fixture.Create<HouseNumber>(),
                _boxNumber,
                _addressGeometry ?? _fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                _postalCode,
                isCompleted: true,
                _isRemoved,
                _parentAddressPersistentLocalId);

            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            return @event;
        }
    }
}
