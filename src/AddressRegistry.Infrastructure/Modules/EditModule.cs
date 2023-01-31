namespace AddressRegistry.Infrastructure.Modules
{
    using Address;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class EditModule : Module
    {
        private readonly IConfiguration _configuration;

        public EditModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;

            var projectionsConnectionString = _configuration.GetConnectionString("Sequences");

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
            builder.RegisterModule(new CommandHandlingModule(_configuration));

            builder.RegisterSnapshotModule(_configuration);

            builder
                .RegisterType<SqlPersistentLocalIdGenerator>()
                .As<IPersistentLocalIdGenerator>();
        }
    }
}
