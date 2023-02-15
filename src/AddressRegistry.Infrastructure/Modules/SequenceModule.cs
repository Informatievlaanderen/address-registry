namespace AddressRegistry.Infrastructure.Modules
{
    using Address;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class SequenceModule : Module
    {
        public SequenceModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var projectionsConnectionString = configuration.GetConnectionString("Sequences");

            services
                .AddDbContext<SequenceContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence)
                    ));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SqlPersistentLocalIdGenerator>()
                .As<IPersistentLocalIdGenerator>();
        }
    }
}
