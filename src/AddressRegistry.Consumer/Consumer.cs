namespace AddressRegistry.Consumer
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

    public class Consumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IIdempotentConsumer<IdempotentConsumerContext> _idempotentConsumer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Consumer> _logger;

        public Consumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IIdempotentConsumer<IdempotentConsumerContext> idempotentConsumer,
            ILoggerFactory loggerFactory)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _idempotentConsumer = idempotentConsumer;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Consumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);
            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameKafkaProjection().Handlers));

            try
            {
                await _idempotentConsumer.ConsumeContinuously(async (message, context) =>
                {
                    _logger.LogInformation("Handling next message");

                    await commandHandlingProjector
                        .ProjectAsync(commandHandler, message, stoppingToken)
                        .ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(Consumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
