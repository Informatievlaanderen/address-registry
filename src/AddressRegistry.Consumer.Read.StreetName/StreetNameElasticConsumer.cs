namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Municipality;
    using Postal;
    using Projections;
    using Projections.Elastic;

    public class StreetNameElasticConsumer : BackgroundService
    {
        private readonly IConsumer _consumer;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;
        private readonly IStreetNameElasticsearchClient _elasticsearchClient;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<StreetNameElasticConsumer> _logger;

        public StreetNameElasticConsumer(
            [KeyFilter(nameof(StreetNameElasticConsumer))] IConsumer consumer,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory,
            IStreetNameElasticsearchClient elasticsearchClient,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _consumer = consumer;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;
            _elasticsearchClient = elasticsearchClient;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _postalConsumerContextFactory = postalConsumerContextFactory;
            _hostApplicationLifetime = hostApplicationLifetime;

            _logger = loggerFactory.CreateLogger<StreetNameElasticConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var latestItemProjector = new ConnectedProjector<StreetNameConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameSearchProjections(
                    _elasticsearchClient,
                    _municipalityConsumerContextFactory,
                    _postalConsumerContextFactory)
                    .Handlers));

            try
            {
                await _consumer.ConsumeContinuously(async (message, messageContext) =>
                {
                    _logger.LogInformation("Handling next message");

                    await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(stoppingToken);

                    await latestItemProjector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    await context.UpdateProjectionState(typeof(StreetNameElasticConsumer).FullName, messageContext.Offset, stoppingToken);
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
