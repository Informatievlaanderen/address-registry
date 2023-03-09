namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Assert
{
    using System.Threading.Tasks;
    using FluentAssertions;

    internal class AndWhichContinuationConstraint<TResult, TAssertion, TWhich> : AndWhichConstraint<TAssertion, Task<TWhich>>
       where TAssertion : TaskContinuationAssertion<TResult, TAssertion>
    {
        public Task<TWhich> Continuation => Which;

        public AndWhichContinuationConstraint(TAssertion parentConstraint, Task<TWhich> matchedConstraint)
            : base(parentConstraint, matchedConstraint) { }
    }
}
