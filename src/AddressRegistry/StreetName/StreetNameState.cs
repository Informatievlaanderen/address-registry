namespace AddressRegistry.StreetName
{
    using Events;

    public partial class StreetName
    {
        public StreetNamePersistentLocalId PersistentLocalId { get; private set; }
        public bool IsRemoved { get; private set; }
        public StreetNameStatus Status { get; private set; }

        private StreetName()
        {
            Register<StreetNameWasImported>(When);
            Register<StreetNameWasApproved>(When);
            Register<StreetNameWasRemoved>(When);
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
    }
}
