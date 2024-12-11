namespace AddressRegistry.Projector.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Consumer;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("consumers")]
    public class ConsumersController : ApiController
    {
        private const string? ConsumerConnectionStringKey = "Consumer";
        private const string? ConsumerReadMunicipalityConnectionStringKey = "ConsumerMunicipality";
        private const string? ConsumerReadStreetNameConnectionStringKey = "ConsumerStreetName";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            await using var sqlConnection =
                new SqlConnection(configuration.GetConnectionString(ConsumerConnectionStringKey));
            var consumerResult =
                sqlConnection.QueryFirstAsync<DateTimeOffset>(
                    $"SELECT TOP(1) [{nameof(ProcessedMessage.DateProcessed)}] FROM [{Schema.Consumer}].[{IdempotentConsumerContext.ProcessedMessageTable}] ORDER BY [{nameof(ProcessedMessage.DateProcessed)}] DESC");

            await using var sqlConsumerReadMunicipalityLatestItemConnection =
                new SqlConnection(configuration.GetConnectionString(ConsumerReadMunicipalityConnectionStringKey));
            var municipalityLatestItemResult =
                sqlConsumerReadMunicipalityLatestItemConnection.QueryFirstAsync<DateTimeOffset>(
                    $"SELECT TOP(1) [{nameof(MunicipalityLatestItem.VersionTimestamp)}] FROM [{Schema.ConsumerReadMunicipality}].[{MunicipalityItemConfiguration.TableName}] ORDER BY [{nameof(MunicipalityLatestItem.VersionTimestamp)}] DESC");

             await using var sqlConsumerReadStreetNameLatestItemConnection =
                 new SqlConnection(configuration.GetConnectionString(ConsumerReadStreetNameConnectionStringKey));
             var streetNameLatestItemResult =
                 sqlConsumerReadStreetNameLatestItemConnection.QueryFirstAsync<DateTimeOffset>(
                    $"SELECT TOP(1) [{nameof(StreetNameLatestItem.VersionTimestamp)}] FROM [{Schema.ConsumerReadStreetName}].[{StreetNameLatestItemConfiguration.TableName}] ORDER BY [{nameof(StreetNameLatestItem.VersionTimestamp)}] DESC");

            await Task.WhenAll(consumerResult, municipalityLatestItemResult, streetNameLatestItemResult);

            return Ok(new object[]
            {
                new
                {
                    Name = "Consumer van straatnaam",
                    LastProcessedMessage = consumerResult.Result
                },
                new
                {
                    Name = "Consumer read van gemeente detail",
                    LastProcessedMessage = municipalityLatestItemResult.Result
                },
                new
                {
                    Name = "Consumer read van straatnaam detail",
                    LastProcessedMessage = streetNameLatestItemResult.Result
                }
            });
        }
    }
}
