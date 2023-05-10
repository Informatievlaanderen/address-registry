namespace AddressRegistry.Tests
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using NodaTime;
    using Xunit;

    public sealed class SnapshotManagerTests
    {
        [Fact(Skip = "Tool to test SnapshotManager.")]
        public async Task ToolToTestSnapshotManager()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.basisregisters.staging-vlaanderen.be/v2/adressen");
            var proxy = new OsloProxy(httpClient);
            var snapshotManager = new SnapshotManager(new NullLoggerFactory(), proxy, SnapshotManagerOptions.Create("1", "1"));
            var result = await snapshotManager.FindMatchingSnapshot(
                "50083",
                Instant.FromDateTimeOffset(DateTimeOffset.Parse("2022-03-23T14:24:04+01:00")),
                null,
                123,
                throwStaleWhenGone: false,
                CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
