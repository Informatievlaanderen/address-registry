namespace AddressRegistry.Consumer.Read.Municipality
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class MunicipalityLatestItemConsumer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConsumer _consumer;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly ILogger<MunicipalityLatestItemConsumer> _logger;

        public MunicipalityLatestItemConsumer(
            IHostApplicationLifetime hostApplicationLifetime,
            [KeyFilter(nameof(MunicipalityLatestItemConsumer))] IConsumer consumer,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            ILoggerFactory loggerFactory)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _consumer = consumer;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;

            _logger = loggerFactory.CreateLogger<MunicipalityLatestItemConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var latestItemProjector = new ConnectedProjector<MunicipalityConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new MunicipalityLatestItemProjections().Handlers));

            try
            {
                await _consumer.ConsumeContinuously(async (message, messageContext) =>
                {
                    await ConsumeHandler(latestItemProjector, message, messageContext);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(MunicipalityLatestItemConsumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task ConsumeHandler(ConnectedProjector<MunicipalityConsumerContext> latestItemProjector, object message, MessageContext messageContext)
        {
            _logger.LogInformation("Handling next message");

            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync();

            await latestItemProjector.ProjectAsync(context, message).ConfigureAwait(false);

            await context.UpdateProjectionState(typeof(MunicipalityLatestItemConsumer).FullName, messageContext.Offset, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
