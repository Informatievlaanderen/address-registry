namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class ScoreableObjectMatcherBase<TBuilder, TResult> : MatcherBase<TBuilder, TResult>
        where TBuilder : class, IProvidesRepresentationsForScoring
        where TResult : class, IScoreable
    {
        public override IReadOnlyList<TResult> BuildResults(TBuilder? input)
        {
            var results = BuildResultsInternal(input) ?? new List<TResult>();

            var representationsForScoring = input?
                .GetMatchRepresentationsForScoring()
                .ToList();

            // representationsForScoring are all possible strings of the input
            // scoreableObject are all objects found by the matchers
            // if one of the fuzzy scores is 100, it means the scoreableObject was a perfect match with the input,
            // in which case we should not take the average but set it at a 100

            foreach (var scoreableObject in results)
            {
                var scores = representationsForScoring
                    ?.Where(representationForScoring => !string.IsNullOrWhiteSpace(scoreableObject.ScoreableProperty))
                    .Select(representationForScoring => scoreableObject.ScoreableProperty!.FuzzyScore(representationForScoring))
                    .ToList();

                scoreableObject.Score = scores.Any(x => x == 100) ? 100 : scores.Average(x => x);
            }


            return results;
        }

        protected abstract IReadOnlyList<TResult>? BuildResultsInternal(TBuilder? builder);
    }
}
