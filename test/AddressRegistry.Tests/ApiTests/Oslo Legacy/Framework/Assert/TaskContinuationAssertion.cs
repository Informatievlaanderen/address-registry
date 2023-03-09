namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Assert
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;

    internal abstract class TaskContinuationAssertion<TResult, TAssertion> : Assertions<Task<TResult>, TAssertion>
            where TAssertion : TaskContinuationAssertion<TResult, TAssertion>
    {
        protected TaskContinuationAssertion(Task<TResult> subject)
            : base(subject) { }

        protected override string Identifier => typeof(TResult).Name;

        protected AndContinuationConstraint<TResult, TAssertion> AndContinuation()
            => new AndContinuationConstraint<TResult, TAssertion>(this as TAssertion);

        protected AndWhichContinuationConstraint<TResult, TAssertion, TWhich> AndWhichContinuation<TWhich>(Task<TWhich> newSubject)
            => new AndWhichContinuationConstraint<TResult, TAssertion, TWhich>(this as TAssertion, newSubject);

        protected void AddAssertion(Action<TResult> assertion)
        {
            Subject.ContinueWith(previous =>
            {
                assertion(previous.Result);

                return previous.Result;
            });
        }

        public AndWhichContinuationConstraint<TResult, TAssertion, AggregateException> ThrowException()
        {
            var newSubject = Subject.ContinueWith(previous =>
            {
                AssertingThat("an exeception was thrown");
                previous.Exception.Should().NotBeNull();

                return previous.Exception;
            });

            return AndWhichContinuation(newSubject);
        }
    }
}
