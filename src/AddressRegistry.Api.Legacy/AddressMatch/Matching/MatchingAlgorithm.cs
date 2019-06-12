namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
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
        private IMatcher<TMatchBuilder, TResult>[] _matchingSteps;

        private IMatcher<TMatchBuilder, TResult> CurrentStep
        {
            get { return _currentStepIndex >= 0 && _currentStepIndex < _matchingSteps.Length ? _matchingSteps[_currentStepIndex] : null; }
        }

        public MatchingAlgorithm(params IMatcher<TMatchBuilder, TResult>[] matchingSteps)
        {
            if (matchingSteps == null || matchingSteps.Length == 0)
                throw new ArgumentException("Provide at least 1 step");

            _currentStepIndex = 0;
            _matchingSteps = matchingSteps;
        }

        public virtual IReadOnlyList<TResult> Process(TMatchBuilder builder)
        {
            TMatchBuilder next = CurrentStep?.DoMatch(builder);
            while (CurrentStep.Proceed && _matchingSteps.Length > ++_currentStepIndex)
                next = CurrentStep.DoMatch(next);

            //go back to the last succesfull step
            while (CurrentStep != null && !CurrentStep.IsMatch)
                _currentStepIndex--;

            return CurrentStep?.BuildResults(next);
        }
    }
}
