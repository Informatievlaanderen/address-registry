using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda
{
    using System.Reflection;
    using Abstractions;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Syndication;
    using TicketingService.Proxy.HttpProxy;

    public class SqsBackOfficeHandlerFunction : FunctionBase
    {
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

            services.AddHttpProxyTicketing(configuration.GetSection("TicketingService")["InternalBaseUrl"]);

            // RETRY POLICY
            var maxRetryCount = int.Parse(configuration.GetSection("RetryPolicy")["MaxRetryCount"]);
            var startingDelaySeconds = int.Parse(configuration.GetSection("RetryPolicy")["StartingRetryDelaySeconds"]);

            var lambdaHandlerRetryPolicy = new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds);
            builder.RegisterInstance(lambdaHandlerRetryPolicy).As<ICustomRetryPolicy>();

            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            builder
                .RegisterModule(new DataDogModule(configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new ConsumerModule(configuration, services, loggerFactory))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(configuration, services, loggerFactory))
                .RegisterModule(new SyndicationModule(configuration, services, loggerFactory));


            builder.RegisterEventstreamModule(configuration);
            builder.RegisterSnapshotModule(configuration);

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });

            builder.RegisterAssemblyTypes(typeof(MessageHandler).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
