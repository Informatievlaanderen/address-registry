namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using System.Threading.Tasks;
    using FluentAssertions;

    public class AndWhichContinuationConstraint<TResult, TAssertion, TWhich> : AndWhichConstraint<TAssertion, Task<TWhich>>
       where TAssertion : TaskContinuationAssertion<TResult, TAssertion>
    {
        public AndWhichContinuationConstraint(TAssertion parentConstraint, Task<TWhich> matchedConstraint) : base(parentConstraint, matchedConstraint)
        {
        }

        public Task<TWhich> Continuation
        {
            get
            {
                return Which;
            }
        }
    }
}