namespace AddressRegistry.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using Xunit;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;

    public sealed class SnapshotManagerTests
    {
        [Fact(Skip = "Tool to test SnapshotManager.")]
        public async Task T()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.basisregisters.staging-vlaanderen.be/v2/adressen");
            var proxy = new OsloProxy(httpClient);
            var snapshotManager = new SnapshotManager(new NullLoggerFactory(), proxy, SnapshotManagerOptions.Create("1", "1"));
            var result = await snapshotManager.FindMatchingSnapshot(
                "50083",
                Instant.FromDateTimeOffset(DateTimeOffset.Parse("2022-03-23T14:24:04+01:00")),
                throwStaleWhenGone: false,
                CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
