namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class ScoreableObjectMatcherBase<TBuilder, TResult> : MatcherBase<TBuilder, TResult>
        where TBuilder : class, IProvidesRepresentationsForScoring?
        where TResult : class, IScoreable
    {
        public override IReadOnlyList<TResult>? BuildResults(TBuilder? builder)
        {
            var results = BuildResultsInternal(builder) ?? new List<TResult>();

            var representationsForScoring = builder?
                .GetMatchRepresentationsForScoring()
                .ToList();

            foreach (var scoreableObject in results)
                scoreableObject.Score = representationsForScoring
                    .Average(a => scoreableObject.ScoreableProperty.FuzzyScore(a));

            return results;
        }

        protected abstract IReadOnlyList<TResult>? BuildResultsInternal(TBuilder? builder);
    }
}
