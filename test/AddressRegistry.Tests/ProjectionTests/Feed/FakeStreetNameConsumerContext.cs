namespace AddressRegistry.Tests.ProjectionTests.Feed
{
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.StreetName;
    using Microsoft.EntityFrameworkCore;

    public sealed class FakeStreetNameConsumerContext : StreetNameConsumerContext
    {
        public FakeStreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
            : base(options)
        { }

        public override void Dispose()
        {
            //DO NOTHING
        }

        public override ValueTask DisposeAsync()
        {
            //DO NOTHING
            return ValueTask.CompletedTask;
        }
    }
}
