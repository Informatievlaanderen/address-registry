namespace AddressRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using AddressDomain = Address.Events;
    using StreetName = StreetName.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string _addressTopicKey = "AddressTopic";

        public ProducerProjections(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var topic = $"{configuration[_addressTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {_addressTopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            #region AddressAggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameIncomplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameNotOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressHouseNumberWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressOfficialAssignmentWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPositionWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPositionWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStatusWasCorrectedToRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStatusWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStreetNameWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStreetNameWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToNotOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasPositioned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), ct);
            });
            #endregion

            // StreetName Aggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });
        }

        private async Task Produce<T>(Guid guid, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, guid.ToString("D"), message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private async Task Produce<T>(int persistentLocalId, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, persistentLocalId.ToString(), message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
