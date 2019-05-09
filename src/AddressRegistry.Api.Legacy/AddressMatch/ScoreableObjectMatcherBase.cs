namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class ScoreableObjectMatcherBase<TBuilder, TResult> : MatcherBase<TBuilder, TResult>
        where TBuilder : IProvidesRepresentationsForScoring
        where TResult : IScoreable
    {
        public override IReadOnlyList<TResult> BuildResults(TBuilder builder)
        {
            IReadOnlyList<TResult> results = BuildResultsInternal(builder);

            List<string> representationsForScoring = builder.GetMatchRepresentationsForScoring().ToList();

            foreach (IScoreable scoreableObject in results)
                scoreableObject.Score = representationsForScoring.Average(a => scoreableObject.ScoreableProperty.FuzzyScore(a));

            return results;
        }

        protected abstract IReadOnlyList<TResult> BuildResultsInternal(TBuilder builder);
    }
}
