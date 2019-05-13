namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using System.Threading.Tasks;
    using FluentAssertions;

    public class AndContinuationConstraint<TResult, TAssertion> : AndConstraint<TAssertion>
        where TAssertion:TaskContinuationAssertion<TResult, TAssertion>
    {
        public AndContinuationConstraint(TAssertion parentConstraint) : base(parentConstraint)
        {
        }

        public Task<TResult> Continuation
        {
            get
            {
                return And.Subject;
            }
        }
    }
}