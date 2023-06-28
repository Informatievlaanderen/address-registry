namespace AddressRegistry.Tests.ProjectionTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using FluentAssertions.Execution;
    using Xunit.Abstractions;

    public abstract class KafkaProjectionTest<TContext, TProjection> : AddressRegistryTest
        where TProjection : ConnectedProjection<TContext>
    {
        private ConnectedProjectionScenario<TContext> _inner;

        protected KafkaProjectionTest(ITestOutputHelper testOutputHelper)
            :base (testOutputHelper)
        {
        }

        protected ConnectedProjectionScenario<TContext> Given(params object[] messages)
        {
            var projection = CreateProjection();
            var resolver = ConcurrentResolve.WhenEqualToHandlerMessageType(projection.Handlers);

            _inner = new ConnectedProjectionScenario<TContext>(resolver)
                .Given(messages);

            return _inner;
        }

        public async Task Then(Func<TContext, Task> assertions)
        {
            var test = CreateTest(assertions);

            var context = CreateContext();

            foreach (var message in test.Messages)
            {
                await new ConnectedProjector<TContext>(test.Resolver)
                    .ProjectAsync(context, message);
            }

            var result = await test.Verification(context, CancellationToken.None);
            if (result.Failed)
            {
                throw new AssertionFailedException(result.Message);
            }
        }

        private ConnectedProjectionTestSpecification<TContext> CreateTest(Func<TContext, Task> assertions)
        {
            return _inner.Verify(async context =>
            {
                try
                {
                    await assertions(context);
                    return VerificationResult.Pass();
                }
                catch (Exception e)
                {
                    return VerificationResult.Fail(e.Message);
                }
            });
        }

        protected abstract TContext CreateContext();
        protected abstract TProjection CreateProjection();
    }
}
