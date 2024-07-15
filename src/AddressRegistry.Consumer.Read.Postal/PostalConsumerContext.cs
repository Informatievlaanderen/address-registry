namespace AddressRegistry.Consumer.Read.Postal
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Projections;

    public class PostalConsumerContext : RunnerDbContext<PostalConsumerContext>
    {
        public DbSet<PostalLatestItem> PostalLatestItems { get; set; }

        // This needs to be here to please EF
        public PostalConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public PostalConsumerContext(DbContextOptions<PostalConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadPostal;
    }

    public class ConsumerContextFactory : IDesignTimeDbContextFactory<PostalConsumerContext>
    {
        public PostalConsumerContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerPostalAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<PostalConsumerContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadPostal, Schema.ConsumerReadPostal);
                })
                .UseExtendedSqlServerMigrations();

            return new PostalConsumerContext(builder.Options);
        }
    }

    public static class PostalLatestItemsExtensions
    {
        public static async Task<PostalLatestItem> FindAndUpdate(
            this PostalConsumerContext context,
            string postalCode,
            Action<PostalLatestItem> updateFunc,
            CancellationToken ct)
        {
            var postalLatestItem = await context
                .PostalLatestItems
                .FindAsync(postalCode, cancellationToken: ct);

            if (postalLatestItem == null)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(postalLatestItem);

            return postalLatestItem;
        }

        private static ProjectionItemNotFoundException<PostalLatestItemProjections> DatabaseItemNotFound(string postalCode)
            => new ProjectionItemNotFoundException<PostalLatestItemProjections>(postalCode);
    }
}
