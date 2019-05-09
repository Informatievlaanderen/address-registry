namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;
    using System.Collections.Generic;

    public interface IMatcher<TMatchBuilder, out TResult>
    {
        TMatchBuilder DoMatch(TMatchBuilder input);

        bool IsMatch { get; }
        bool Proceed { get; }

        IReadOnlyList<TResult> BuildResults(TMatchBuilder input);
    }

    internal abstract class MatcherBase<TMatchBuilder, TResult> : IMatcher<TMatchBuilder, TResult>
    {
        private bool? _isMatch = null;
        private bool? _proceed = null;

        public bool IsMatch
        {
            get
            {
                if (!_isMatch.HasValue)
                    throw new Exception("Run DoMatch first before getting IsMatch");

                return _isMatch.Value;
            }
            private set
            {
                _isMatch = value;
            }
        }

        public bool Proceed
        {
            get
            {
                if (!_proceed.HasValue)
                    throw new Exception("Run DoMatch first before getting Proceed");

                return _proceed.Value;
            }
            private set
            {
                _proceed = value;
            }
        }

        public abstract IReadOnlyList<TResult> BuildResults(TMatchBuilder builder);

        protected abstract TMatchBuilder DoMatchInternal(TMatchBuilder builder);

        protected abstract bool IsValidMatch(TMatchBuilder builder);

        protected abstract bool ShouldProceed(TMatchBuilder builder);

        public TMatchBuilder DoMatch(TMatchBuilder builder)
        {
            TMatchBuilder nextBuilder = DoMatchInternal(builder);

            Proceed = ShouldProceed(nextBuilder);
            IsMatch = IsValidMatch(nextBuilder);

            return nextBuilder;
        }
    }
}
