using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AddressRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Reflection;
    using Abstractions;
    using Abstractions.SqsRequests;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Elastic.Apm;
    using Elastic.Apm.DiagnosticSource;
    using Elastic.Apm.EntityFrameworkCore;
    using Elastic.Apm.SqlClient;
    using ElasticApm.MediatR;
    using Infrastructure;
    using Infrastructure.Modules;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Projections.Syndication;
    using TicketingService.Proxy.HttpProxy;

    public class Function : FunctionBase
    {
        public Function() : base(new List<Assembly> { typeof(ApproveAddressSqsRequest).Assembly })
        {
            Agent.Setup(new AgentComponents());
            Agent.Subscribe(new SqlClientDiagnosticSubscriber(),
                new EfCoreDiagnosticsSubscriber(),
                new HttpDiagnosticsSubscriber(),
                new MediatrDiagnosticsSubscriber());
        }

        protected override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new ContainerBuilder();

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterSnapshotModule(configuration);

            builder.RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.Register(_ => configuration)
                .AsSelf()
                .As<IConfiguration>()
                .SingleInstance();

            services.AddHttpProxyTicketing(configuration.GetSection("TicketingService")["InternalBaseUrl"]);

            // RETRY POLICY
            var maxRetryCount = int.Parse(configuration.GetSection("RetryPolicy")["MaxRetryCount"]);
            var startingDelaySeconds = int.Parse(configuration.GetSection("RetryPolicy")["StartingRetryDelaySeconds"]);

            builder.Register(_ => new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds))
                .As<ICustomRetryPolicy>()
                .AsSelf()
                .SingleInstance();

            // JSON Serialization
            JsonConvert.DefaultSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings;

            builder
                .RegisterModule(new DataDogModule(configuration))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SequenceModule(configuration, services, loggerFactory))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(configuration, services, loggerFactory))
                .RegisterModule(new SyndicationModule(configuration, services, loggerFactory));

            builder.RegisterType<IdempotentCommandHandler>()
                .As<IIdempotentCommandHandler>()
                .AsSelf()
                .InstancePerLifetimeScope();

            services.ConfigureIdempotency(
                configuration
                    .GetSection(IdempotencyConfiguration.Section)
                    .Get<IdempotencyConfiguration>()
                    .ConnectionString!,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                loggerFactory);

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
