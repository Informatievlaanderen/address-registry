namespace AddressRegistry.Consumer.Read.Postal
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

    public class PostalLatestItemConsumer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConsumer _consumer;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly ILogger<PostalLatestItemConsumer> _logger;

        public PostalLatestItemConsumer(
            IHostApplicationLifetime hostApplicationLifetime,
            [KeyFilter(nameof(PostalLatestItemConsumer))] IConsumer consumer,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            ILoggerFactory loggerFactory)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _consumer = consumer;
            _postalConsumerContextFactory = postalConsumerContextFactory;

            _logger = loggerFactory.CreateLogger<PostalLatestItemConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var latestItemProjector = new ConnectedProjector<PostalConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new PostalLatestItemProjections().Handlers));

            try
            {
                await _consumer.ConsumeContinuously(async (message, messageContext) =>
                {
                    _logger.LogInformation("Handling next message");

                    await using var context = await _postalConsumerContextFactory.CreateDbContextAsync(stoppingToken);

                    await latestItemProjector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    await context.UpdateProjectionState(typeof(PostalLatestItemConsumer).FullName, messageContext.Offset, stoppingToken);
                    await context.SaveChangesAsync(CancellationToken.None);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Critical error occurred in {nameof(PostalLatestItemConsumer)}");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
