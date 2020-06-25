namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
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
