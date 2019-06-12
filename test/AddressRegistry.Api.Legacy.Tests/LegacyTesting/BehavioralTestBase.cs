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
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Options;
    using Mocking;
    using Newtonsoft.Json;
    using Projections.Legacy;
    using Projections.Syndication;
    using Xunit.Abstractions;

    public abstract class BehavioralTestBase : HandlerTestBase
    {
        protected Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification> _kadRrService;
        protected Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification> _latest;
        protected SyndicationContextMemory _syndicationContext;
        protected LegacyContextMemory _legacyContext;
        protected Random _random;
        private readonly AddressMatchController _controller;

        public BehavioralTestBase(ITestOutputHelper testOutputHelper, Action<string> logAction, Formatting logFormatting = Formatting.Indented, bool disableArrangeLogging = false, bool disableActLogging = false, bool disableAssertionLogging = false) :
            base(testOutputHelper, logAction, logFormatting, disableArrangeLogging: disableArrangeLogging, disableActLogging: disableActLogging, disableAssertionLogging: disableAssertionLogging)
        {
            _random = new Random();

            _kadRrService = new Mocking<IKadRrService, KadRrServiceSetup, KadRrServiceVerification>();
            _latest = new Mocking<ILatestQueries, LatestQueriesSetup, LatestQueriesVerification>();
            _controller = new AddressMatchController();
            _syndicationContext = new SyndicationContextMemory("DB", InMemoryDatabaseRootRoot);
            _legacyContext = new LegacyContextMemory("DB", InMemoryDatabaseRootRoot);
        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder)
        {
            base.ConfigureMocks(containerBuilder);

            containerBuilder.RegisterInstance(_kadRrService.Object).As<IKadRrService>();
            containerBuilder.RegisterInstance(_latest.Object).As<ILatestQueries>();
            containerBuilder.RegisterInstance(_syndicationContext).As<SyndicationContext>();
            containerBuilder.RegisterInstance(_legacyContext).As<LegacyContext>();
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
                }),
                _syndicationContext,
                request);
        }
    }

    public class LegacyContextMemory : LegacyContext
    {
        private readonly string _name;
        private readonly InMemoryDatabaseRoot _root;

        public LegacyContextMemory(string name, InMemoryDatabaseRoot root)
        {
            _name = name;
            _root = root;
        }
        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(_name, _root);
    }

    public class SyndicationContextMemory : SyndicationContext
    {
        private readonly string _name;
        private readonly InMemoryDatabaseRoot _root;

        public SyndicationContextMemory(string name, InMemoryDatabaseRoot root)
        {
            _name = name;
            _root = root;
        }
        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(_name, _root);
    }
}
