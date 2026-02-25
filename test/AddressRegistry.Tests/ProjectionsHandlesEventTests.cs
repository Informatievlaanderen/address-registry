namespace AddressRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Consumer.Read.Municipality;
    using Consumer.Read.Postal;
    using Consumer.Read.StreetName;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.IO;
    using Producer;
    using Producer.Snapshot.Oslo;
    using Projections.AddressMatch;
    using Projections.BackOffice;
    using Projections.Elastic;
    using Projections.Elastic.AddressList;
    using Projections.Elastic.AddressSearch;
    using Projections.Extract;
    using Projections.Extract.AddressExtract;
    using Projections.Feed;
    using Projections.Feed.AddressFeed;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.LatestItem;
    using Projections.Integration.Merger;
    using Projections.Integration.Version;
    using Projections.LastChangedList;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetailV2WithParent;
    using Projections.Legacy.AddressSyndication;
    using Projections.Wfs;
    using Projections.Wfs.AddressWfsV2;
    using Projections.Wms;
    using Projections.Wms.AddressWmsItemV3;
    using SqlStreamStore;
    using StreetName.Events;
    using Xunit;
    using HouseNumberLabelUpdater = Projections.Wms.AddressWmsItemV3.HouseNumberLabelUpdater;
    using ProducerContext = Producer.Snapshot.Oslo.ProducerContext;

    public sealed class ProjectionsHandlesEventsTests
    {
        private readonly IEnumerable<Type> _eventsToExclude = [typeof(StreetNameSnapshot), typeof(StreetNameSnapshotWasRequested)];
        private readonly IList<Type> _eventTypes;

        public ProjectionsHandlesEventsTests()
        {
            _eventTypes = DiscoverEventTypes();
        }

        private IList<Type> DiscoverEventTypes()
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => InfrastructureEventsTests.GetAssemblyTypesSafe(a)
                    .Any(t => t.Name == "DomainAssemblyMarker"));

            if (domainAssembly == null)
            {
                return Enumerable.Empty<Type>().ToList();
            }

            return domainAssembly.GetTypes()
                            .Where(t => t is { IsClass: true, Namespace: not null }
                                        && IsEventNamespace(t)
                                        && IsNotCompilerGenerated(t)
                                        && t.GetCustomAttributes(typeof(EventNameAttribute), true).Length != 0)
                            .Except(_eventsToExclude)
                            .ToList();
        }

        private static bool IsEventNamespace(Type t) => t.Namespace?.EndsWith("StreetName.Events") ?? false;
        private static bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

        [Theory]
        [MemberData(nameof(GetProjectionsToTest))]
        public void ProjectionsHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest)
        {
            AssertHandleEvents(projectionsToTest);
        }

        [Fact]
        public void MergerProjectionHandlesEvents()
        {
            var projectionsToTest = new List<ConnectedProjection<IntegrationContext>>
            {
                new AddressMergerItemProjections()
            };

            AssertHandleEvents(projectionsToTest, [
                typeof(AddressBoxNumberWasCorrectedV2),
                typeof(AddressBoxNumbersWereCorrected),
                typeof(AddressDeregulationWasCorrected),
                typeof(AddressHouseNumberWasCorrectedV2),
                typeof(AddressHouseNumberWasReaddressed),
                typeof(AddressPositionWasChanged),
                typeof(AddressPositionWasCorrectedV2),
                typeof(AddressPostalCodeWasChangedV2),
                typeof(AddressPostalCodeWasCorrectedV2),
                typeof(AddressRegularizationWasCorrected),
                typeof(AddressRemovalWasCorrected),
                typeof(AddressWasApproved),
                typeof(AddressWasCorrectedFromApprovedToProposed),
                typeof(AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected),
                typeof(AddressWasCorrectedFromRejectedToProposed),
                typeof(AddressWasCorrectedFromRetiredToCurrent),
                typeof(AddressWasDeregulated),
                typeof(AddressWasMigratedToStreetName),
                typeof(AddressWasProposedBecauseOfReaddress),
                typeof(AddressWasProposedV2),
                typeof(AddressWasRegularized),
                typeof(AddressWasRejected),
                typeof(AddressWasRejectedBecauseHouseNumberWasRejected),
                typeof(AddressWasRejectedBecauseHouseNumberWasRetired),
                typeof(AddressWasRejectedBecauseOfMunicipalityMerger),
                typeof(AddressWasRejectedBecauseOfReaddress),
                typeof(AddressWasRejectedBecauseStreetNameWasRejected),
                typeof(AddressWasRejectedBecauseStreetNameWasRetired),
                typeof(AddressWasRemovedBecauseHouseNumberWasRemoved),
                typeof(AddressWasRemovedBecauseStreetNameWasRemoved),
                typeof(AddressWasRemovedV2),
                typeof(AddressWasRetiredBecauseHouseNumberWasRetired),
                typeof(AddressWasRetiredBecauseOfMunicipalityMerger),
                typeof(AddressWasRetiredBecauseOfReaddress),
                typeof(AddressWasRetiredBecauseStreetNameWasRejected),
                typeof(AddressWasRetiredBecauseStreetNameWasRetired),
                typeof(AddressWasRetiredV2),
                typeof(MigratedStreetNameWasImported),
                typeof(StreetNameHomonymAdditionsWereCorrected),
                typeof(StreetNameHomonymAdditionsWereRemoved),
                typeof(StreetNameNamesWereChanged),
                typeof(StreetNameNamesWereCorrected),
                typeof(StreetNameSnapshot),
                typeof(StreetNameWasApproved),
                typeof(StreetNameWasCorrectedFromApprovedToProposed),
                typeof(StreetNameWasCorrectedFromRejectedToProposed),
                typeof(StreetNameWasCorrectedFromRetiredToCurrent),
                typeof(StreetNameWasImported),
                typeof(StreetNameWasReaddressed),
                typeof(StreetNameWasRejected),
                typeof(StreetNameWasRejectedBecauseOfMunicipalityMerger),
                typeof(StreetNameWasRemoved),
                typeof(StreetNameWasRenamed),
                typeof(StreetNameWasRetired),
                typeof(StreetNameWasRetiredBecauseOfMunicipalityMerger),
                typeof(StreetNameSnapshotWasRequested)
            ]);
        }

        [Fact]
        public void BackOfficeProjectionHandlesEvents()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "DelayInSeconds", "10" }
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var projectionsToTest = new List<ConnectedProjection<BackOfficeProjectionsContext>>
            {
                new BackOfficeProjections(
                    Mock.Of<IDbContextFactory<BackOfficeContext>>(),
                    configurationRoot)
            };

            AssertHandleEvents(projectionsToTest, [
                typeof(AddressBoxNumberWasCorrectedV2),
                typeof(AddressBoxNumbersWereCorrected),
                typeof(AddressDeregulationWasCorrected),
                typeof(AddressHouseNumberWasCorrectedV2),
                typeof(AddressHouseNumberWasReaddressed),
                typeof(AddressPositionWasChanged),
                typeof(AddressPositionWasCorrectedV2),
                typeof(AddressPostalCodeWasChangedV2),
                typeof(AddressPostalCodeWasCorrectedV2),
                typeof(AddressRegularizationWasCorrected),
                typeof(AddressRemovalWasCorrected),
                typeof(AddressWasApproved),
                typeof(AddressWasCorrectedFromApprovedToProposed),
                typeof(AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected),
                typeof(AddressWasCorrectedFromRejectedToProposed),
                typeof(AddressWasCorrectedFromRetiredToCurrent),
                typeof(AddressWasDeregulated),
                typeof(AddressWasMigratedToStreetName),
                typeof(AddressWasRegularized),
                typeof(AddressWasRejected),
                typeof(AddressWasRejectedBecauseHouseNumberWasRejected),
                typeof(AddressWasRejectedBecauseHouseNumberWasRetired),
                typeof(AddressWasRejectedBecauseOfMunicipalityMerger),
                typeof(AddressWasRejectedBecauseOfReaddress),
                typeof(AddressWasRejectedBecauseStreetNameWasRejected),
                typeof(AddressWasRejectedBecauseStreetNameWasRetired),
                typeof(AddressWasRemovedBecauseHouseNumberWasRemoved),
                typeof(AddressWasRemovedBecauseStreetNameWasRemoved),
                typeof(AddressWasRetiredBecauseHouseNumberWasRetired),
                typeof(AddressWasRetiredBecauseOfMunicipalityMerger),
                typeof(AddressWasRetiredBecauseOfReaddress),
                typeof(AddressWasRetiredBecauseStreetNameWasRejected),
                typeof(AddressWasRetiredBecauseStreetNameWasRetired),
                typeof(AddressWasRetiredV2),
                typeof(MigratedStreetNameWasImported),
                typeof(StreetNameHomonymAdditionsWereCorrected),
                typeof(StreetNameHomonymAdditionsWereRemoved),
                typeof(StreetNameNamesWereChanged),
                typeof(StreetNameNamesWereCorrected),
                typeof(StreetNameSnapshot),
                typeof(StreetNameWasApproved),
                typeof(StreetNameWasCorrectedFromApprovedToProposed),
                typeof(StreetNameWasCorrectedFromRejectedToProposed),
                typeof(StreetNameWasCorrectedFromRetiredToCurrent),
                typeof(StreetNameWasImported),
                typeof(StreetNameWasReaddressed),
                typeof(StreetNameWasRejected),
                typeof(StreetNameWasRejectedBecauseOfMunicipalityMerger),
                typeof(StreetNameWasRemoved),
                typeof(StreetNameWasRenamed),
                typeof(StreetNameWasRetired),
                typeof(StreetNameWasRetiredBecauseOfMunicipalityMerger),
                typeof(StreetNameSnapshotWasRequested)
            ]);
        }

        public static IEnumerable<object[]> GetProjectionsToTest()
        {
            yield return [new List<ConnectedProjection<LegacyContext>>
            {
                new AddressDetailProjectionsV2WithParent(),
                new AddressSyndicationProjections()
            }];

            yield return [new List<ConnectedProjection<FeedContext>>
            {
                new AddressFeedProjections(Mock.Of<IChangeFeedService>())
            }];

            yield return [new List<ConnectedProjection<AddressMatchContext>>
            {
                new Projections.AddressMatch.AddressDetailV2WithParent.AddressDetailProjectionsV2WithParent(),
            }];

            yield return [new List<ConnectedProjection<WfsContext>>
            {
                new AddressWfsV2Projections(new WKBReader(), new Projections.Wfs.AddressWfsV2.HouseNumberLabelUpdater())
            }];

            yield return [new List<ConnectedProjection<WmsContext>>
            {
                new AddressWmsItemV3Projections(new WKBReader(), new HouseNumberLabelUpdater())
            }];

            yield return [new List<ConnectedProjection<LastChangedListContext>>
            {
                new LastChangedListProjections(Mock.Of<ICacheValidator>())
            }];

            yield return [new List<ConnectedProjection<IntegrationContext>>
            {
                new AddressLatestItemProjections(Mock.Of<IOptions<IntegrationOptions>>()),
                new AddressVersionProjections(Mock.Of<IOptions<IntegrationOptions>>(), Mock.Of<IEventsRepository>())
            }];

            yield return [new List<ConnectedProjection<ExtractContext>>
            {
                new AddressExtractProjectionsV2(Mock.Of<IReadonlyStreamStore>(), new EventDeserializer((_, _) => new object()), new OptionsWrapper<ExtractConfig>(new ExtractConfig()), Encoding.UTF8, new WKBReader())
            }];

            yield return [new List<ConnectedProjection<ProducerContext>>
            {
                new ProducerProjections(Mock.Of<IProducer>(), Mock.Of<ISnapshotManager>(), "")
            }];

            yield return [new List<ConnectedProjection<AddressRegistry.Producer.ProducerContext>>
            {
                new ProducerMigrateReaddressFixProjections(Mock.Of<IProducer>(), Mock.Of<IStreamStore>()),
                new ProducerProjectionsV3(Mock.Of<IProducer>(), Mock.Of<IStreamStore>())
            }];

            yield return [new List<ConnectedProjection<ElasticRunnerContext>>
                {
                    new AddressSearchProjections(Mock.Of<IAddressSearchElasticClient>(),
                        Mock.Of<IDbContextFactory<MunicipalityConsumerContext>>(),
                        Mock.Of<IDbContextFactory<PostalConsumerContext>>(),
                        Mock.Of<IDbContextFactory<StreetNameConsumerContext>>()),
                    new AddressListProjections(Mock.Of<IAddressListElasticClient>(),
                        Mock.Of<IDbContextFactory<MunicipalityConsumerContext>>(),
                        Mock.Of<IDbContextFactory<PostalConsumerContext>>(),
                        Mock.Of<IDbContextFactory<StreetNameConsumerContext>>())
                }
            ];
        }

        private void AssertHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest, IList<Type>? eventsToExclude = null)
        {
            var eventsToCheck = _eventTypes.Except(eventsToExclude ?? Enumerable.Empty<Type>()).ToList();
            foreach (var projection in projectionsToTest)
            {
                projection.Handlers.Should().NotBeEmpty();
                foreach (var eventType in eventsToCheck)
                {
                    var eventHandlersCount = projection.Handlers.Count(x => x.Message.GetGenericArguments().First() == eventType);
                    eventHandlersCount.Should().BeGreaterThan(0, $"The event {eventType.Name} is not handled by the projection {projection.GetType().Name}");
                    eventHandlersCount.Should().BeLessOrEqualTo(1, $"The event {eventType.Name} has multiple handlers in the projection {projection.GetType().Name}");
                }
            }
        }
    }
}
