namespace AddressRegistry.Api.Legacy.AddressMatch.V1.Matching
{
    using System.Collections.Generic;

    public interface IScoreable
    {
        string? ScoreableProperty { get; }
        double Score { get; set; }
    }

    internal interface IProvidesRepresentationsForScoring
    {
        IEnumerable<string> GetMatchRepresentationsForScoring();
    }
}
