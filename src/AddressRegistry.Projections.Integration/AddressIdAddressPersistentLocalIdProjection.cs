namespace AddressRegistry.Projections.Integration
{
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie adres id's")]
    [ConnectedProjectionDescription("Projectie die de relatie legt tussen adres id en adres persistent local id.")]
    public sealed class AddressIdAddressPersistentLocalIdRelationProjections : ConnectedProjection<IntegrationContext>
    {
        public AddressIdAddressPersistentLocalIdRelationProjections()
        {
            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.AddressIdAddressPersistentLocalIds.AddAsync(
                    new AddressIdAddressPersistentLocalIdRelation
                    {
                        AddressId = message.Message.AddressId,
                        PersistentLocalId = message.Message.PersistentLocalId
                    }
                );
            });
        }
    }
}
