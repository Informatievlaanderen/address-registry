namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Mocking
{
    using ILatestQueries = Api.Oslo.AddressMatch.V2.Matching.ILatestQueries;

    public class LatestQueriesVerification : MockingVerification<AddressRegistry.Api.Oslo.AddressMatch.V1.Matching.ILatestQueries>
    {
    }

    public class LatestQueriesV2Verification : MockingVerification<ILatestQueries>
    {
    }
}
