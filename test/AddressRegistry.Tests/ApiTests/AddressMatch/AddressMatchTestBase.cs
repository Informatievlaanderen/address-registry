namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using Microsoft.EntityFrameworkCore.Storage;
    using Xunit.Abstractions;

    public abstract class AddressMatchTestBase
    {
        internal static readonly InMemoryDatabaseRoot InMemoryDatabaseRootRoot = new InMemoryDatabaseRoot();

        protected readonly ITestOutputHelper TestOutputHelper;

        protected AddressMatchTestBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }
    }
}
