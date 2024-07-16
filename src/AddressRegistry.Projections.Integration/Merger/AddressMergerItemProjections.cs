namespace AddressRegistry.Projections.Integration.Merger
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using StreetName.Events;

    [ConnectedProjectionName("Integratie adres fusie item")]
    [ConnectedProjectionDescription("Projectie die de fusie adres data voor de integratie database bijhoudt.")]
    public class AddressMergerItemProjections : ConnectedProjection<IntegrationContext>
    {
        public AddressMergerItemProjections()
        {
            When<Envelope<AddressWasProposedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = new AddressMergerItem(message.Message.AddressPersistentLocalId, message.Message.MergedAddressPersistentLocalId);

                await context
                    .AddressMergerItems
                    .AddAsync(item, ct);
            });
        }
    }
}
