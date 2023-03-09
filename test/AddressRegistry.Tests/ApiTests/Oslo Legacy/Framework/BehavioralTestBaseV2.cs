namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.AddressMatch.Requests;
    using Api.Oslo.AddressMatch.Responses;
    using Api.Oslo.AddressMatch.V2;
    using Api.Oslo.AddressMatch.V2.Matching;
    using Api.Oslo.Infrastructure.Options;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Mocking;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class BehavioralTestBaseV2 : HandlerTestBase
    {
        private readonly AddressMatchContextMemoryV2 _addresMatchContext;

        protected readonly Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification> Latest;
        protected readonly Random Random;

        protected BehavioralTestBaseV2(
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
            Latest = new Mocking<ILatestQueries, LatestQueriesV2Setup, LatestQueriesV2Verification>();

            _addresMatchContext = new AddressMatchContextMemoryV2();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder)
        {
            base.ConfigureMocks(containerBuilder);

            containerBuilder.RegisterInstance(Latest.Object).As<ILatestQueries>();
            containerBuilder.RegisterInstance(_addresMatchContext).As<AddressMatchContextV2>();
        }

        protected async Task<AddressMatchCollection> Send(AddressMatchRequest request)
        {
            var handler = new AddressMatchHandlerV2(Latest.Object, _addresMatchContext,
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

    public class AddressMatchContextMemoryV2 : AddressMatchContextV2
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("DB", HandlerTestBase.InMemoryDatabaseRootRoot);
    }
}
