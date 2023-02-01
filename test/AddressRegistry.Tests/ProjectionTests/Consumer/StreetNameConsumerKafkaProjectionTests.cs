namespace AddressRegistry.Tests.ProjectionTests.Consumer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Consumer.Projections;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public class StreetNameConsumerKafkaProjectionTests : KafkaProjectionTest<CommandHandler, StreetNameKafkaProjection>
    {
        private readonly Mock<FakeCommandHandler> _mockCommandHandler;

        public StreetNameConsumerKafkaProjectionTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _mockCommandHandler = new Mock<FakeCommandHandler>();
        }

        private class StreetNameEventsGenerator : IEnumerable<object[]>
        {
            private readonly Fixture? _fixture;

            public StreetNameEventsGenerator()
            {
                _fixture = new Fixture();
                _fixture.Customize(new InfrastructureCustomization());
                _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();
                var municipalityId = _fixture.Create<MunicipalityId>().ToString();
                var nisCode = _fixture.Create<string>();
                var retirementDate = _fixture.Create<Instant>().ToString();
                var provenance = new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Unknown.ToString(),
                    Organisation.DigitaalVlaanderen.ToString(),
                    new Reason("")
                );

                var result = new List<object[]>
                {
                    new object[] { new StreetNameWasMigratedToMunicipality(municipalityId, nisCode, _fixture.Create<Guid>().ToString("D"), streetNamePersistentLocalId, _fixture.Create<StreetNameStatus>().ToString(), null, null,null,null, true, false, provenance ) },
                    new object[] { new StreetNameWasProposedV2(municipalityId, nisCode, null, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasApproved(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromApprovedToProposed(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasRejected(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromRejectedToProposed(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasRetiredV2(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromRetiredToCurrent(municipalityId, streetNamePersistentLocalId, provenance) }

                };
                return result.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(StreetNameEventsGenerator))]
        public async Task HandleMessage(object obj)
        {
            if (obj is not IQueueMessage queueMessage)
            {
                throw new InvalidOperationException("Parameter is not an IQueueMessage");
            }

            var command = StreetNameKafkaProjection.GetCommand(queueMessage);
            _mockCommandHandler.Setup(commandHandler => commandHandler.Handle(command, default)).Returns(Task.CompletedTask);

            Given(command);
            await Then(ct =>
            {
                _mockCommandHandler.Verify(commandHandler => commandHandler.Handle(It.IsAny<IHasCommandProvenance>(), default), Times.AtMostOnce());
                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasMigratedToMunicipality_ThenImportMunicipality()
        {
            var @event = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<PersistentLocalId>(),
                "Proposed",
                "NL",
                "FR",
                Fixture.Create<IDictionary<string, string>>(),
                Fixture.Create<IDictionary<string, string>>(),
                true,
                false,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Insert.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is ImportMigratedStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasProposedV2_ThenImportStreetName()
        {
            var @event = new StreetNameWasProposedV2(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<IDictionary<string, string>>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Insert.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is ImportStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasApproved_ThenApproveStreetName()
        {
            var @event = new StreetNameWasApproved(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is ApproveStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasCorrectedFromApprovedToProposed_ThenCorrectStreetNameApproval()
        {
            var @event = new StreetNameWasCorrectedFromApprovedToProposed(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectStreetNameApproval), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasCorrectedFromRejectedToProposed_ThenCorrectStreetNameRejection()
        {
            var @event = new StreetNameWasCorrectedFromRejectedToProposed(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectStreetNameRejection), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRejected_ThenRejectStreetName()
        {
            var @event = new StreetNameWasRejected(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is RejectStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRetiredV2_ThenRetireStreetName()
        {
            var @event = new StreetNameWasRetiredV2(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is RetireStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasCorrectedFromRetiredToCurrent_ThenCorrectStreetNameRetirement()
        {
            var @event = new StreetNameWasCorrectedFromRetiredToCurrent(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectStreetNameRetirement), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        protected override CommandHandler CreateContext()
        {
            return _mockCommandHandler.Object;
        }

        protected override StreetNameKafkaProjection CreateProjection()
        {
            return new StreetNameKafkaProjection();
        }
    }

    public class FakeCommandHandler : CommandHandler
    {
        public FakeCommandHandler() : base(null, new NullLoggerFactory())
        { }
    }
}
