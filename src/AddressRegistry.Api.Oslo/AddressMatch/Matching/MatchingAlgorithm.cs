namespace AddressRegistry.Api.Oslo.AddressMatch.Matching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements a matching algorithm using a pipeline of IMatchers
    /// </summary>
    /// <typeparam name="TMatchBuilder"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class MatchingAlgorithm<TMatchBuilder, TResult>
        where TMatchBuilder : class
    {
        private int _currentStepIndex;
        private readonly IMatcher<TMatchBuilder, TResult>[] _matchingSteps;

        private IMatcher<TMatchBuilder, TResult>? CurrentStep =>
            _currentStepIndex >= 0 && _currentStepIndex < _matchingSteps.Length
                ? _matchingSteps[_currentStepIndex]
                : null;

        protected MatchingAlgorithm(params IMatcher<TMatchBuilder, TResult>[] matchingSteps)
        {
            if (matchingSteps == null || matchingSteps.Length == 0)
                throw new ArgumentException("Provide at least 1 step");

            _currentStepIndex = 0;
            _matchingSteps = matchingSteps;
        }

        public virtual IReadOnlyList<TResult>? Process(TMatchBuilder builder)
        {
            var next = CurrentStep?.DoMatch(builder);
            while (CurrentStep?.Proceed != null && CurrentStep.Proceed.Value && _matchingSteps.Length > ++_currentStepIndex)
                next = CurrentStep.DoMatch(next);

            //go back to the last succesfull step
            while (CurrentStep?.IsMatch != null && !CurrentStep.IsMatch.Value)
                _currentStepIndex--;

            return CurrentStep?.BuildResults(next);
        }
    }
}
