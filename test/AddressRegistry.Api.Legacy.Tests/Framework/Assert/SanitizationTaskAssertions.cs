namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;

    internal class SanitizationTaskAssertions<T> : TaskContinuationAssertion<List<T>, SanitizationTaskAssertions<T>>
    {
        public SanitizationTaskAssertions(Task<List<T>> subject)
            : base(subject) { }

        public AndContinuationConstraint<List<T>, SanitizationTaskAssertions<T>> HaveCount(int expectedCount)
        {
            AddAssertion(result=>
            {
                AssertingThat($"the result has {expectedCount} items");

                result.Should().HaveCount(expectedCount);
            });

            return AndContinuation();
        }

        public Task<T> First()
            => Subject.ContinueWith(previous => previous.Result.First());

        public Task<T> Second()
            => Subject.ContinueWith(previous => previous.Result.ElementAt(1));
    }
}
