namespace AddressRegistry.Tests.ProjectionTests.Consumer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Projections;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using Tests.BackOffice.Infrastructure;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public class StreetNameConsumerKafkaProjectionTests : KafkaProjectionTest<CommandHandler, StreetNameKafkaProjection>
    {
        private readonly Mock<FakeCommandHandler> _mockCommandHandler;
        private readonly BackOfficeContext _backOfficeContext;

        public StreetNameConsumerKafkaProjectionTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _mockCommandHandler = new Mock<FakeCommandHandler>();

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
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
                    new object[] { new StreetNameWasProposedV2(municipalityId, nisCode, new Dictionary<string, string>(), streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasProposedForMunicipalityMerger(municipalityId, nisCode, _fixture.Create<StreetNameStatus>().ToString(), new Dictionary<string, string>(), new Dictionary<string, string>(), streetNamePersistentLocalId, [], provenance) },
                    new object[] { new StreetNameWasApproved(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromApprovedToProposed(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasRejected(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromRejectedToProposed(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasRetiredV2(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasCorrectedFromRetiredToCurrent(municipalityId, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasRemovedV2(municipalityId, streetNamePersistentLocalId, provenance) }

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
            await Then(_ =>
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is ImportMigratedStreetName), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is ImportStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasProposedForMunicipalityMerger_ThenImportStreetName()
        {
            var @event = new StreetNameWasProposedForMunicipalityMerger(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<string>(),
                Fixture.Create<IDictionary<string, string>>(),
                Fixture.Create<IDictionary<string, string>>(),
                Fixture.Create<PersistentLocalId>(),
                [],
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is ImportStreetName), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is ApproveStreetName), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is CorrectStreetNameApproval), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is CorrectStreetNameRejection), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is RejectStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenRejectStreetNameBecauseOfMunicipalityMerger()
        {
            var @event = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                [],
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            var firstMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId,
                Fixture.Create<int>(),
                Fixture.Create<int>(),
                Fixture.Create<int>());
            var secondMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId,
                firstMunicipalityMergerAddress.OldAddressPersistentLocalId + 1,
                firstMunicipalityMergerAddress.NewStreetNamePersistentLocalId + 1,
                firstMunicipalityMergerAddress.NewAddressPersistentLocalId + 1);
            var otherMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId + 100,
                firstMunicipalityMergerAddress.OldAddressPersistentLocalId + 100,
                firstMunicipalityMergerAddress.NewStreetNamePersistentLocalId + 100,
                firstMunicipalityMergerAddress.NewAddressPersistentLocalId + 100);
            _backOfficeContext.MunicipalityMergerAddresses.Add(firstMunicipalityMergerAddress);
            _backOfficeContext.MunicipalityMergerAddresses.Add(secondMunicipalityMergerAddress);
            _backOfficeContext.MunicipalityMergerAddresses.Add(otherMunicipalityMergerAddress);
            await _backOfficeContext.SaveChangesAsync();

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(
                        It.Is<IHasCommandProvenance>(y =>
                            y is RejectStreetNameBecauseOfMunicipalityMerger
                            && ((RejectStreetNameBecauseOfMunicipalityMerger)y).NewAddressPersistentLocalIdsByMerged.Count == 2),
                        CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is RetireStreetName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenRetireStreetNameBecauseOfMunicipalityMerger()
        {
            var @event = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                [],
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            var firstMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId,
                Fixture.Create<int>(),
                Fixture.Create<int>(),
                Fixture.Create<int>());
            var secondMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId,
                firstMunicipalityMergerAddress.OldAddressPersistentLocalId + 1,
                firstMunicipalityMergerAddress.NewStreetNamePersistentLocalId + 1,
                firstMunicipalityMergerAddress.NewAddressPersistentLocalId + 1);
            var otherMunicipalityMergerAddress = new MunicipalityMergerAddress(
                @event.PersistentLocalId + 100,
                firstMunicipalityMergerAddress.OldAddressPersistentLocalId + 100,
                firstMunicipalityMergerAddress.NewStreetNamePersistentLocalId + 100,
                firstMunicipalityMergerAddress.NewAddressPersistentLocalId + 100);
            _backOfficeContext.MunicipalityMergerAddresses.Add(firstMunicipalityMergerAddress);
            _backOfficeContext.MunicipalityMergerAddresses.Add(secondMunicipalityMergerAddress);
            _backOfficeContext.MunicipalityMergerAddresses.Add(otherMunicipalityMergerAddress);
            await _backOfficeContext.SaveChangesAsync();

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(
                        It.Is<IHasCommandProvenance>(y =>
                            y is RetireStreetNameBecauseOfMunicipalityMerger
                            && ((RetireStreetNameBecauseOfMunicipalityMerger)y).NewAddressPersistentLocalIdsByMerged.Count == 2),
                        CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRenamed_ThenRenameAndRetireBecauseOfRenameStreetName()
        {
            var @event = new StreetNameWasRenamed(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(cmd =>
                        cmd is RenameStreetName
                            && ((RenameStreetName)cmd).Provenance.Timestamp.ToString() == @event.Provenance.Timestamp), CancellationToken.None),
                    Times.Once);
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(cmd =>
                        cmd is RetireStreetNameBecauseOfRename
                        && ((RetireStreetNameBecauseOfRename)cmd).Provenance.Timestamp.ToString() == @event.Provenance.Timestamp), CancellationToken.None),
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
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is CorrectStreetNameRetirement), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenStreetNameWasRemovedV2_ThenStreetNameIsRemoved()
        {
            var @event = new StreetNameWasRemovedV2(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.AddressRegistry.ToString(),
                    Modification.Delete.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(y => y is RemoveStreetName), CancellationToken.None),
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
            var backOfficeContextFactory = new Mock<IDbContextFactory<BackOfficeContext>>();
            backOfficeContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_backOfficeContext);

            return new StreetNameKafkaProjection(backOfficeContextFactory.Object);
        }
    }

    public class FakeCommandHandler : CommandHandler
    {
        public FakeCommandHandler() : base(null, new NullLoggerFactory())
        { }
    }
}
