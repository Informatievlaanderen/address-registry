namespace AddressRegistry.StreetName
{
    using Events;

    public partial class StreetName
    {
        public StreetNamePersistentLocalId PersistentLocalId { get; private set; }
        public NisCode MigratedNisCode { get; private set; }
        public bool IsRemoved { get; private set; }
        public StreetNameStatus Status { get; private set; }

        public StreetNameAddresses StreetNameAddresses { get; } = new StreetNameAddresses();

        private StreetName()
        {
            Register<StreetNameWasImported>(When);
            Register<MigratedStreetNameWasImported>(When);

            Register<StreetNameWasApproved>(When);
            Register<StreetNameWasRemoved>(When);
            Register<AddressWasMigratedToStreetName>(When);
            Register<AddressWasProposedV2>(When);
        }

        private void When(MigratedStreetNameWasImported @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            MigratedNisCode = new NisCode(@event.NisCode);
            Status = @event.StreetNameStatus;
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
    }
}
