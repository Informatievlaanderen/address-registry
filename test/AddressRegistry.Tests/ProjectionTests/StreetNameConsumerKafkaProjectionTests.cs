namespace AddressRegistry.Tests.ProjectionTests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Consumer.Projections;
using global::AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using StreetName;
using Xunit;
using Xunit.Abstractions;
using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

public class StreetNameConsumerKafkaProjectionTests : KafkaProjectionTest<CommandHandler, StreetNameKafkaProjection>
{
    private readonly Mock<CommandHandler> _mock;

    public StreetNameConsumerKafkaProjectionTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _mock = new Mock<CommandHandler>();
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
                    new object[] { new StreetNameWasMigratedToMunicipality(municipalityId, nisCode, _fixture.Create<StreetNameId>(), streetNamePersistentLocalId, _fixture.Create<StreetNameStatus>().ToString(), null, null,null,null, true, false, provenance ) },
                    new object[] { new StreetNameWasProposedV2(municipalityId, nisCode, null, streetNamePersistentLocalId, provenance) },
                    new object[] { new StreetNameWasApproved(municipalityId, streetNamePersistentLocalId, provenance) }
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
        _mock.Setup(commandHandler => commandHandler.Handle(command, default))
            .Returns(Task.CompletedTask);

        Given(command);
        await Then(ct =>
            {
                _mock.Verify(commandHandler => commandHandler.Handle(It.IsAny<IHasCommandProvenance>(), default), Times.AtMostOnce());
                return Task.CompletedTask;
            });
    }

    protected override CommandHandler CreateContext()
    {
        return new CommandHandler(Container.BeginLifetimeScope(), new Mock<ILoggerFactory>().Object);
    }

    protected override StreetNameKafkaProjection CreateProjection()
    {
        return new StreetNameKafkaProjection();
    }
}


