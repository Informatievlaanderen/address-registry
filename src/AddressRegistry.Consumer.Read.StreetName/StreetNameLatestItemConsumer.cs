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

        public StreetNameLatestItemConsumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<StreetNameConsumerContext> kafkaIdemIdempotencyConsumer)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _kafkaIdemIdempotencyConsumer = kafkaIdemIdempotencyConsumer;

            _logger = loggerFactory.CreateLogger<StreetNameLatestItemConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var latestItemProjector = new ConnectedProjector<StreetNameConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameLatestItemProjections().Handlers));

            try
            {
                await _kafkaIdemIdempotencyConsumer.ConsumeContinuously(async (message, context) =>
                {
                    _logger.LogInformation("Handling next message");

                    // await commandHandlingProjector.ProjectAsync(commandHandler, message, stoppingToken)
                    //     .ConfigureAwait(false);
                    await latestItemProjector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);

                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
