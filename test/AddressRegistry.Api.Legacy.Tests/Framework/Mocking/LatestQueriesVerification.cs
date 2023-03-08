namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
{
    using Legacy.AddressMatch.V1.Matching;

    public class LatestQueriesVerification : MockingVerification<ILatestQueries>
    {
    }

    public class LatestQueriesV2Verification : MockingVerification<Legacy.AddressMatch.V2.Matching.ILatestQueries>
    {
    }
}
