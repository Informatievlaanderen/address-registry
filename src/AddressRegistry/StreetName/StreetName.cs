namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;

    public partial class StreetName : AggregateRootEntity
    {
        public static readonly Func<StreetName> Factory = () => new StreetName();

        public static StreetName Register(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            MunicipalityId municipalityId,
            StreetNameStatus streetNameStatus)
        {
            var streetName = Factory();
            streetName.ApplyChange(new StreetNameWasImported(streetNamePersistentLocalId, municipalityId, streetNameStatus));
            return streetName;
        }

        public void ApproveStreetName()
        {
            ApplyChange(new StreetNameWasApproved(PersistentLocalId));
        }

        public void RemoveStreetName()
        {
            ApplyChange(new StreetNameWasRemoved(PersistentLocalId));
            //TODO: remove addresses?
        }

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion
    }
}
