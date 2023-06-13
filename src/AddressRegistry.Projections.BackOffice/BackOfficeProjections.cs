namespace AddressRegistry.Projections.BackOffice
{
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using StreetName.Events;

    public sealed class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<AddressWasProposedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    message.Message.AddressPersistentLocalId, message.Message.StreetNamePersistentLocalId, cancellationToken);
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    message.Message.AddressPersistentLocalId, message.Message.StreetNamePersistentLocalId, cancellationToken);
            });
        }
    }
}
