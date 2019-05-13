namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;

    public class SanitizationTaskAssertions<T> : TaskContinuationAssertion<List<T>, SanitizationTaskAssertions<T>>
    {
        public SanitizationTaskAssertions(Task<List<T>> subject) : base(subject)
        {
        }

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
        {
            return Subject.ContinueWith(previous =>
            {
                //LogAction("Asserting the first item");
                return previous.Result.First();
            });
        }

        public Task<T> Second()
        {
            return Subject.ContinueWith(previous =>
            {
                //LogAction("Asserting the second item");
                return previous.Result.ElementAt(1);
            });
        }
    }
}
