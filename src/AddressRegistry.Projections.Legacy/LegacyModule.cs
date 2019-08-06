namespace AddressRegistry.Projections.Legacy
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class LegacyModule : Module
    {
        public LegacyModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<LegacyModule>();
            var connectionString = configuration.GetConnectionString("LegacyProjections");

            services
                .AddDbContext<LegacyContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Legacy, Schema.Legacy);
                        //sqlServerOptions.UseNetTopologySuite();
                    }));

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(LegacyContext), Schema.Legacy, MigrationTables.Legacy);
        }

        protected override void Load(ContainerBuilder builder)
            => builder.RegisterType<RegistryAtomFeedReader>().As<IRegistryAtomFeedReader>();
    }
}
