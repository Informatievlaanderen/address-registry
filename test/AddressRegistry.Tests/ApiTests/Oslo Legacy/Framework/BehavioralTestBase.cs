namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.Oslo.AddressMatch.Requests;
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using AddressRegistry.Api.Oslo.AddressMatch.V1;
    using AddressRegistry.Api.Oslo.AddressMatch.V1.Matching;
    using AddressRegistry.Api.Oslo.Infrastructure.Options;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Mocking;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class BehavioralTestBase : HandlerTestBase
    {
        private readonly AddressMatchContextMemory _addresMatchContext;
        private readonly BuildingContextMemory _buildingContext;

        protected readonly Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> KadRrService;
        protected readonly Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> Latest;
        protected readonly Random Random;

        protected BehavioralTestBase(
            ITestOutputHelper testOutputHelper,
            Action<string> logAction,
            Formatting logFormatting = Formatting.Indented,
            bool disableArrangeLogging = false,
            bool disableAssertionLogging = false)
            : base(
                testOutputHelper,
                logAction,
                logFormatting,
                disableArrangeLogging,
                disableAssertionLogging)
        {
            Random = new Random();
            KadRrService = new Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification>();
            Latest = new Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification>();

            _addresMatchContext = new AddressMatchContextMemory();
            _buildingContext = new BuildingContextMemory();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder)
        {
            base.ConfigureMocks(containerBuilder);

            containerBuilder.RegisterInstance(KadRrService.Object).As<IKadRrService>();
            containerBuilder.RegisterInstance(Latest.Object).As<ILatestQueries>();
            containerBuilder.RegisterInstance(_addresMatchContext).As<AddressMatchContext>();
            containerBuilder.RegisterInstance(_buildingContext).As<BuildingContext>();
        }

        protected async Task<AddressMatchCollection> Send(AddressMatchRequest request)
        {
            var handler = new AddressMatchHandler(KadRrService.Object, Latest.Object, _addresMatchContext, _buildingContext,
                new OptionsWrapper<ResponseOptions>(new ResponseOptions
                {
                    DetailUrl = "detail/{0}",
                    GemeenteDetailUrl = "gemeentedetail/{0}",
                    StraatnaamDetailUrl = "straatnaamdetail/{0}",
                    PostInfoDetailUrl = "postinfodetail/{0}",
                    MaxStreetNamesThreshold = 100,
                    SimilarityThreshold = 75.0
                }));
            return await handler.Handle(request, CancellationToken.None);
        }
    }

    public class AddressMatchContextMemory : AddressMatchContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("DB", HandlerTestBase.InMemoryDatabaseRootRoot);
    }

    public class BuildingContextMemory : BuildingContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("DB", HandlerTestBase.InMemoryDatabaseRootRoot);
    }
}
