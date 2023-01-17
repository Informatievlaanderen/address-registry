namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class StreetNameLatestItemConsumer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IIdempotentConsumer<StreetNameConsumerContext> _kafkaIdemIdempotencyConsumer;
        private readonly ILogger<StreetNameLatestItemConsumer> _logger;
        private readonly StreetNameCommandHandler _commandHandler;

        public StreetNameLatestItemConsumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<StreetNameConsumerContext> kafkaIdemIdempotencyConsumer)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _kafkaIdemIdempotencyConsumer = kafkaIdemIdempotencyConsumer;

            _logger = loggerFactory.CreateLogger<StreetNameLatestItemConsumer>();
            _commandHandler = new StreetNameCommandHandler(lifetimeScope, loggerFactory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var latestItemProjector = new ConnectedProjector<StreetNameConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameLatestItemProjections().Handlers));

            var commandHandlingProjector = new ConnectedProjector<StreetNameCommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameCommandHandlingProjections().Handlers));

            try
            {
                await _kafkaIdemIdempotencyConsumer.ConsumeContinuously(async (message, context) =>
                {
                    _logger.LogInformation("Handling next message");

                    await commandHandlingProjector
                        .ProjectAsync(_commandHandler, message, stoppingToken)
                        .ConfigureAwait(false);

                    await latestItemProjector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(StreetNameLatestItemConsumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
