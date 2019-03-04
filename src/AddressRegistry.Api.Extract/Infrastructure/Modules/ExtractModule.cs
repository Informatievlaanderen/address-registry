namespace AddressRegistry.Api.Extract.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AddressRegistry.Infrastructure;
    using Projections.Extract;

    public class ExtractModule : Module
    {
        public ExtractModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ExtractModule>();
            var projectionsConnectionString = configuration.GetConnectionString("ExtractProjections");

            services
                .AddDbContext<ExtractContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                    }));

            logger.LogInformation(
                "Added {Context} to services:" + Environment.NewLine +
                "\tSchema: {Schema}" + Environment.NewLine +
                "\tMigrationTable: {ProjectionMetaData}.{TableName}",
                nameof(ExtractContext),
                Schema.Extract,
                Schema.Extract, MigrationTables.Extract);
        }
    }
}
