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
            Register<StreetNameWasRejected>(When);
            Register<StreetNameWasRetired>(When);
            Register<StreetNameWasRemoved>(When);
            Register<StreetNameNamesWereCorrected>(When);
            Register<StreetNameHomonymAdditionsWereCorrected>(When);
            Register<StreetNameHomonymAdditionsWereRemoved>(When);
            Register<StreetNameWasCorrectedFromApprovedToProposed>(When);
            Register<StreetNameWasCorrectedFromRejectedToProposed>(When);
            Register<StreetNameWasCorrectedFromRetiredToCurrent>(When);

            Register<AddressWasMigratedToStreetName>(When);
            Register<AddressWasProposedV2>(When);
            Register<AddressWasApproved>(When);
            Register<AddressWasRejected>(When);
            Register<AddressWasRejectedBecauseHouseNumberWasRejected>(When);
            Register<AddressWasRejectedBecauseHouseNumberWasRetired>(When);
            Register<AddressWasRejectedBecauseStreetNameWasRetired>(When);
            Register<AddressWasRetiredV2>(When);
            Register<AddressWasRetiredBecauseHouseNumberWasRetired>(When);
            Register<AddressWasRetiredBecauseStreetNameWasRetired>(When);
            Register<AddressWasRemovedV2>(When);
            Register<AddressWasRemovedBecauseHouseNumberWasRemoved>(When);
            Register<AddressWasRegularized>(When);
            Register<AddressWasDeregulated>(When);
            Register<AddressPositionWasCorrectedV2>(When);
            Register<AddressPostalCodeWasCorrectedV2>(When);
            Register<AddressHouseNumberWasCorrectedV2>(When);
            Register<AddressBoxNumberWasCorrectedV2>(When);
            Register<AddressWasCorrectedFromApprovedToProposed>(When);
            Register<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(When);
            Register<AddressWasCorrectedFromRejectedToProposed>(When);
            Register<AddressWasCorrectedFromRetiredToCurrent>(When);
            Register<AddressRegularizationWasCorrected>(When);
            Register<AddressDeregulationWasCorrected>(When);
            Register<AddressPositionWasChanged>(When);
            Register<AddressPostalCodeWasChangedV2>(When);
            Register<AddressWasRemovedBecauseStreetNameWasRemoved>(When);
        }

        private void When(StreetNameSnapshot @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            MunicipalityId = new MunicipalityId(@event.MunicipalityId);
            MigratedNisCode = string.IsNullOrEmpty(@event.MigratedNisCode) ? null : new NisCode(@event.MigratedNisCode);
            Status = @event.StreetNameStatus;
            IsRemoved = @event.IsRemoved;

            foreach (var address in @event.Addresses.Where(x => !x.ParentId.HasValue))
            {
                var streetNameAddress = new StreetNameAddress(applier: ApplyChange);
                streetNameAddress.RestoreSnapshot(PersistentLocalId, address);

                StreetNameAddresses.Add(streetNameAddress);
            }

            foreach (var address in @event.Addresses.Where(x => x.ParentId.HasValue))
            {
                var parent =
                    StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(address.ParentId!.Value));

                var streetNameAddress = new StreetNameAddress(applier: ApplyChange);
                streetNameAddress.RestoreSnapshot(PersistentLocalId, address);
                streetNameAddress.SetParent(parent);

                StreetNameAddresses.Add(streetNameAddress);
                parent.AddChild(streetNameAddress);
            }
        }

        private void When(StreetNameWasImported @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            Status = @event.StreetNameStatus;
            MunicipalityId = new MunicipalityId(@event.MunicipalityId);
        }

        private void When(MigratedStreetNameWasImported @event)
        {
            PersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            MigratedNisCode = new NisCode(@event.NisCode);
            Status = @event.StreetNameStatus;
            MunicipalityId = new MunicipalityId(@event.MunicipalityId);
        }

        private void When(StreetNameWasApproved @event)
        {
            Status = StreetNameStatus.Current;
        }

        private void When(StreetNameWasRejected @event)
        {
            Status = StreetNameStatus.Rejected;
        }

        private void When(StreetNameWasRetired @event)
        {
            Status = StreetNameStatus.Retired;
        }

        private void When(StreetNameWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(StreetNameNamesWereCorrected @event)
        {
            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                StreetNameAddresses
                    .GetByPersistentLocalId(new AddressPersistentLocalId(addressPersistentLocalId))
                    .Route(@event);
            }
        }

        private void When(StreetNameHomonymAdditionsWereCorrected @event)
        {
            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                StreetNameAddresses
                    .GetByPersistentLocalId(new AddressPersistentLocalId(addressPersistentLocalId))
                    .Route(@event);
            }
        }

        private void When(StreetNameHomonymAdditionsWereRemoved @event)
        {
            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                StreetNameAddresses
                    .GetByPersistentLocalId(new AddressPersistentLocalId(addressPersistentLocalId))
                    .Route(@event);
            }
        }

        private void When(StreetNameWasCorrectedFromApprovedToProposed @event)
        {
            Status = StreetNameStatus.Proposed;
        }

        private void When(StreetNameWasCorrectedFromRejectedToProposed @event)
        {
            Status = StreetNameStatus.Proposed;
        }

        private void When(StreetNameWasCorrectedFromRetiredToCurrent @event)
        {
            Status = StreetNameStatus.Current;
        }

        private void When(AddressWasMigratedToStreetName @event)
        {
            var address = new StreetNameAddress(applier: ApplyChange);
            address.Route(@event);

            if (@event.ParentPersistentLocalId.HasValue)
            {
                var parent =
                    StreetNameAddresses.GetByPersistentLocalId(
                        new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value));
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
                var parent =
                    StreetNameAddresses.GetByPersistentLocalId(
                        new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value));
                parent.AddChild(address);
            }

            StreetNameAddresses.Add(address);
        }

        private void When(AddressWasApproved @event) => RouteToAddress(@event);

        private void When(AddressWasRejected @event) => RouteToAddress(@event);

        private void When(AddressWasRejectedBecauseHouseNumberWasRejected @event) => RouteToAddress(@event);

        private void When(AddressWasRejectedBecauseHouseNumberWasRetired @event) => RouteToAddress(@event);

        private void When(AddressWasRejectedBecauseStreetNameWasRetired @event) => RouteToAddress(@event);

        private void When(AddressWasRetiredV2 @event) => RouteToAddress(@event);

        private void When(AddressWasRetiredBecauseHouseNumberWasRetired @event) => RouteToAddress(@event);

        private void When(AddressWasRetiredBecauseStreetNameWasRetired @event) => RouteToAddress(@event);

        private void When(AddressWasRemovedV2 @event) => RouteToAddress(@event);

        private void When(AddressWasRemovedBecauseHouseNumberWasRemoved @event) => RouteToAddress(@event);

        private void When(AddressWasRegularized @event) => RouteToAddress(@event);

        private void When(AddressWasDeregulated @event) => RouteToAddress(@event);

        private void When(AddressPositionWasCorrectedV2 @event) => RouteToAddress(@event);

        private void When(AddressPostalCodeWasCorrectedV2 @event) => RouteToAddress(@event);

        private void When(AddressHouseNumberWasCorrectedV2 @event) => RouteToAddress(@event);

        private void When(AddressBoxNumberWasCorrectedV2 @event) => RouteToAddress(@event);

        private void When(AddressWasCorrectedFromApprovedToProposed @event) => RouteToAddress(@event);

        private void When(AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected @event) =>
            RouteToAddress(@event);

        private void When(AddressWasCorrectedFromRejectedToProposed @event) => RouteToAddress(@event);

        private void When(AddressWasCorrectedFromRetiredToCurrent @event) => RouteToAddress(@event);

        private void When(AddressRegularizationWasCorrected @event) => RouteToAddress(@event);

        private void When(AddressDeregulationWasCorrected @event) => RouteToAddress(@event);

        private void When(AddressPositionWasChanged @event) => RouteToAddress(@event);

        private void When(AddressPostalCodeWasChangedV2 @event) => RouteToAddress(@event);

        private void When(AddressWasRemovedBecauseStreetNameWasRemoved @event) => RouteToAddress(@event);

        private void RouteToAddress<TEvent>(TEvent @event)
            where TEvent : IHasAddressPersistentLocalId, IStreetNameEvent
        {
            StreetNameAddresses
                .GetByPersistentLocalId(new AddressPersistentLocalId(@event.AddressPersistentLocalId))
                .Route(@event);
        }
    }
}
