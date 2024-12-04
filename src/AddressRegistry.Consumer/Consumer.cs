namespace AddressRegistry.Consumer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class Consumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IIdempotentConsumer<IdempotentConsumerContext> _idempotentConsumer;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Consumer> _logger;

        public Consumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IIdempotentConsumer<IdempotentConsumerContext> idempotentConsumer,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            ILoggerFactory loggerFactory)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _idempotentConsumer = idempotentConsumer;
            _backOfficeContextFactory = backOfficeContextFactory;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Consumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);
            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameKafkaProjection(_backOfficeContextFactory).Handlers));

            try
            {
                await _idempotentConsumer.ConsumeContinuously(async (message, context) =>
                {
                    await ConsumeHandler(commandHandlingProjector, commandHandler, message, context);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(Consumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task ConsumeHandler(ConnectedProjector<CommandHandler> commandHandlingProjector, CommandHandler commandHandler, object message, IdempotentConsumerContext context)
        {
            _logger.LogInformation("Handling next message");

            await commandHandlingProjector
                .ProjectAsync(commandHandler, message)
                .ConfigureAwait(false);

            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
