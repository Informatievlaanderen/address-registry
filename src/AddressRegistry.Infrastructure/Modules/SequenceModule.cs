namespace AddressRegistry.Infrastructure.Modules
{
    using Address;
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class SequenceModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public SequenceModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;

            var projectionsConnectionString = _configuration.GetConnectionString("Sequences");

            _services
                .AddDbContext<SequenceContext>(options => options
                    .UseLoggerFactory(_loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence)
                    ));
        }

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<SqlPersistentLocalIdGenerator>()
                .As<IPersistentLocalIdGenerator>();
        }
    }
}
