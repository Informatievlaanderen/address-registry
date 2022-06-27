namespace AddressRegistry.StreetName
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;

    public partial class StreetName
    {
        public MunicipalityId MunicipalityId { get; private set; }
        public StreetNamePersistentLocalId PersistentLocalId { get; private set; }
        public NisCode? MigratedNisCode { get; private set; }
        public bool IsRemoved { get; private set; }
        public StreetNameStatus Status { get; private set; }

        public bool IsActive => !IsRemoved 
                                && Status != StreetNameStatus.Retired
                                && Status != StreetNameStatus.Rejected;

        public StreetNameAddresses StreetNameAddresses { get; } = new StreetNameAddresses();

        internal StreetName(ISnapshotStrategy snapshotStrategy) : this()
        {
            Strategy = snapshotStrategy;
        }

        private StreetName()
        {
            Register<StreetNameSnapshot>(When);

            Register<StreetNameWasImported>(When);
            Register<MigratedStreetNameWasImported>(When);

            Register<StreetNameWasApproved>(When);
            Register<StreetNameWasRemoved>(When);
            Register<AddressWasMigratedToStreetName>(When);
            Register<AddressWasProposedV2>(When);
            Register<AddressWasApproved>(When);
        }

        private void When(MigratedStreetNameWasImported @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            MigratedNisCode = new NisCode(@event.NisCode);
            Status = @event.StreetNameStatus;
            MunicipalityId = new MunicipalityId(@event.MunicipalityId);
        }

        private void When(StreetNameWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(StreetNameWasApproved @event)
        {
            Status = StreetNameStatus.Current;
        }

        private void When(StreetNameWasImported @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            Status = @event.StreetNameStatus;
            MunicipalityId = new MunicipalityId(@event.MunicipalityId);
        }

        private void When(AddressWasMigratedToStreetName @event)
        {
            var address = new StreetNameAddress(applier: ApplyChange);
            address.Route(@event);

            if (@event.ParentPersistentLocalId.HasValue)
            {
                var parent = StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value));
                parent.AddChild(address);
            }

            StreetNameAddresses.Add(address);
        }

        private void When(AddressWasProposedV2 @event)
        {
            var address = new StreetNameAddress(applier: ApplyChange);
            address.Route(@event);

            if (@event.ParentPersistentLocalId.HasValue)
            {
                var parent = StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value));
                parent.AddChild(address);
            }

            StreetNameAddresses.Add(address);
        }

        private void When(AddressWasApproved @event)
        {
            var addressToApprove = StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(@event.AddressPersistentLocalId));
            addressToApprove.Route(@event);
        }

        private void When(StreetNameSnapshot @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            MigratedNisCode = string.IsNullOrEmpty(@event.MigratedNisCode) ? null : new NisCode(@event.MigratedNisCode);
            Status = @event.StreetNameStatus;
            IsRemoved = @event.IsRemoved;

            foreach (var address in @event.Addresses.Where(x => !x.ParentId.HasValue))
            {
                var streetNameAddress = new StreetNameAddress(applier: ApplyChange);
                streetNameAddress.RestoreSnapshot(address);

                StreetNameAddresses.Add(streetNameAddress);
            }

            foreach (var address in @event.Addresses.Where(x => x.ParentId.HasValue))
            {
                var parent = StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(address.ParentId!.Value));

                var streetNameAddress = new StreetNameAddress(applier: ApplyChange);
                streetNameAddress.RestoreSnapshot(address);
                streetNameAddress.SetParent(parent);

                StreetNameAddresses.Add(streetNameAddress);
                parent.AddChild(streetNameAddress);
            }
        }
    }
}
