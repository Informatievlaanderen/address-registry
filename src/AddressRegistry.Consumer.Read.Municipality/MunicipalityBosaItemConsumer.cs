namespace AddressRegistry.Consumer.Read.Municipality
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class MunicipalityBosaItemConsumer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IConsumer _consumer;
        private readonly ILogger<MunicipalityBosaItemConsumer> _logger;

        public MunicipalityBosaItemConsumer(
            IHostApplicationLifetime hostApplicationLifetime,
            [KeyFilter(nameof(MunicipalityBosaItemConsumer))] IConsumer consumer,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            ILoggerFactory loggerFactory)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _consumer = consumer;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;

            _logger = loggerFactory.CreateLogger<MunicipalityBosaItemConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var projector = new ConnectedProjector<MunicipalityConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new MunicipalityBosaItemProjections().Handlers));

            try
            {
                await _consumer.ConsumeContinuously(async message =>
                {
                    _logger.LogInformation("Handling next message");

                    await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync(stoppingToken);

                    await projector.ProjectAsync(context, message, stoppingToken);

                    await context.SaveChangesAsync(stoppingToken);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(MunicipalityBosaItemConsumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
