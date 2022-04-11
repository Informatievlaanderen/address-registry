namespace AddressRegistry.Consumer.Projections
{
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using StreetName;

    public class StreetNameConsumerProjection : ConnectedProjection<ConsumerContext>
    {
        public StreetNameConsumerProjection()
        {
            When<Envelope<MigratedStreetNameWasImported>>(async (context, message, ct) =>
            {
                await context.StreetNameConsumerItems.AddAsync(new StreetNameConsumerItem
                {
                    StreetNameId = message.Message.StreetNameId,
                    PersistentLocalId = message.Message.StreetNamePersistentLocalId
                }, ct);
            });
        }
    }
}
