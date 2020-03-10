namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using System;
    using System.Threading.Tasks;
    using AddressMatch;
    using AddressMatch.Matching;
    using AddressMatch.Requests;
    using Autofac;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Mocking;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class BehavioralTestBase : HandlerTestBase
    {
        protected Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> _kadRrService;
        protected Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> _latest;
        protected AddressMatchContextMemory _context;
        protected Random _random;
        private readonly AddressMatchController _controller;
        private readonly BuildingContextMemory _buildingContext;

        public BehavioralTestBase(ITestOutputHelper testOutputHelper, Action<string> logAction, Formatting logFormatting = Formatting.Indented, bool disableArrangeLogging = false, bool disableActLogging = false, bool disableAssertionLogging = false) :
            base(testOutputHelper, logAction, logFormatting, disableArrangeLogging: disableArrangeLogging, disableActLogging: disableActLogging, disableAssertionLogging: disableAssertionLogging)
        {
            _random = new Random();

            _kadRrService = new Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification>();
            _latest = new Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification>();
            _controller = new AddressMatchController();
            _context = new AddressMatchContextMemory();
            _buildingContext = new BuildingContextMemory();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder)
        {
            base.ConfigureMocks(containerBuilder);

            containerBuilder.RegisterInstance(_kadRrService.Object).As<IKadRrService>();
            containerBuilder.RegisterInstance(_latest.Object).As<ILatestQueries>();
            containerBuilder.RegisterInstance(_context).As<AddressMatchContext>();
            containerBuilder.RegisterInstance(_buildingContext).As<BuildingContext>();
        }

        public Task<IActionResult> Send(AddressMatchRequest request)
        {
            return _controller.Get(
                _kadRrService.Object,
                _latest.Object,
                new OptionsWrapper<ResponseOptions>(new ResponseOptions
                {
                    DetailUrl = "detail/{0}",
                    GemeenteDetailUrl = "gemeentedetail/{0}",
                    StraatnaamDetailUrl = "straatnaamdetail/{0}",
                    PostInfoDetailUrl = "postinfodetail/{0}",
                    MaxStreetNamesThreshold = 100,
                    SimilarityThreshold = 75.0
                }),
                _context,
                _buildingContext,
                request);
        }
    }

    public class AddressMatchContextMemory : AddressMatchContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("DB", HandlerTestBase.InMemoryDatabaseRootRoot);
    }

    public class BuildingContextMemory : BuildingContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("DB", HandlerTestBase.InMemoryDatabaseRootRoot);
    }
}
