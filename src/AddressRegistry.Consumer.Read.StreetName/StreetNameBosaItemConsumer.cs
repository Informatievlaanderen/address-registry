namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class StreetNameBosaItemConsumer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;
        private readonly IConsumer _kafkaConsumer;
        private readonly ILogger<StreetNameBosaItemConsumer> _logger;

        public StreetNameBosaItemConsumer(
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory,
            ILoggerFactory loggerFactory,
            IConsumer kafkaConsumer)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;
            _kafkaConsumer = kafkaConsumer;

            _logger = loggerFactory.CreateLogger<StreetNameBosaItemConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bosaItemProjections = new ConnectedProjector<StreetNameConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new StreetNameBosaItemProjections().Handlers));

            try
            {
                await _kafkaConsumer.ConsumeContinuously(async (message)=>
                {
                    _logger.LogInformation("Handling next message");

                    await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(stoppingToken);

                    await bosaItemProjections.ProjectAsync(context, message, stoppingToken);

                    await context.SaveChangesAsync(stoppingToken);
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
