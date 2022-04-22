namespace AddressRegistry.StreetName
{
    using Events;
    using Exceptions;

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
            var address = new StreetNameAddress(ApplyChange);
            address.Route(@event);

            if (@event.ParentPersistentLocalId.HasValue)
            {
                if (!StreetNameAddresses.HasPersistentLocalId(new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value), out var parent))
                {
                    throw new ParentAddressNotFoundException();
                }

                parent!.AddChild(address);
            }

            StreetNameAddresses.Add(address);
        }
    }
}
