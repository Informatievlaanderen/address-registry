namespace AddressRegistry.Projections.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using StreetName.Events;

    public sealed class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory, IConfiguration configuration)
        {
            var delayInSeconds = configuration.GetValue("DelayInSeconds", 10);

            When<Envelope<AddressWasProposedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message.CreatedUtc, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    message.Message.AddressPersistentLocalId, message.Message.StreetNamePersistentLocalId, cancellationToken);
            });

            When<Envelope<AddressWasProposedBecauseOfMunicipalityMerger>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message.CreatedUtc, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    message.Message.AddressPersistentLocalId, message.Message.StreetNamePersistentLocalId, cancellationToken);
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message.CreatedUtc, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                    message.Message.AddressPersistentLocalId, message.Message.StreetNamePersistentLocalId, cancellationToken);
            });
        }

        private static async Task DelayProjection(DateTime messageTimestamp, int delayInSeconds, CancellationToken cancellationToken)
        {
            var differenceInSeconds = (DateTime.UtcNow -messageTimestamp).TotalSeconds;
            if (differenceInSeconds < delayInSeconds)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds - differenceInSeconds), cancellationToken);
            }
        }
    }
}
