namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using global::AutoFixture.Dsl;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public class AutoFixtureScenario : Scenario, IEventGeneratingScenarioInitialStateBuilder
    {
        private readonly IFixture _fixture;

        public AutoFixtureScenario(IFixture fixture)
        {
            _fixture = fixture;
        }

        //public IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<IPostprocessComposer<TEvent>, IPostprocessComposer<TEvent>> customize = null)
        //{
        //    IPostprocessComposer<TEvent> composer = _fixture.Build<TEvent>();
        //    if (customize != null)
        //        composer = customize(composer);

        //    var @event = composer.Create();
        //    return new AutoAutoFixtureTestBuilder(Given(identifier, @event), _fixture);
        //}

        /// <summary>
        /// Given an event of type TEvent occurred
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="identifier">The aggregate identifier the events is to be associated with.</param>
        /// <param name="customize">An action that perfoms customizations of the event.</param>
        /// <returns></returns>
        public IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<TEvent, TEvent> customize = null)
        {
            var @event = _fixture.Create<TEvent>();
            if (customize != null)
                @event = customize(@event);

            return new AutoAutoFixtureTestBuilder(Given(identifier, @event), _fixture);
        }

        public IEventGeneratingScenarioGivenStateBuilder Given(IEventCentricTestSpecificationBuilder givenScenario)
        {
            var specification = givenScenario.Build();

            return new AutoAutoFixtureTestBuilder(this.Given(specification.Givens).Given(SetProvenance(specification.Thens)), _fixture);
        }

        private Fact[] SetProvenance(Fact[] facts)
        {
            foreach (var @event in facts)
            {
                if (@event.Event is ISetProvenance)
                {
                    var hasProvenance = @event.Event as IHasProvenance;
                    var setProvenance = @event.Event as ISetProvenance;
                    if(hasProvenance?.Provenance == null)
                        setProvenance.SetProvenance(_fixture.Create<Provenance>());
                }
            }

            return facts;
        }
    }

    public interface IEventGeneratingScenarioInitialStateBuilder : IScenarioInitialStateBuilder
    {
        //IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(Func<IPostprocessComposer<TEvent>, IPostprocessComposer<TEvent>> customize = null);

        /// <summary>
        /// Given an event of type TEvent occurred
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="identifier">The aggregate identifier the events is to be associated with.</param>
        /// <param name="customize">An action that perfoms customizations of the event.</param>
        /// <returns></returns>
        IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<TEvent, TEvent> customize = null);

        /// <summary>
        /// Given an already tested scenario. The Givens and Thens of the specified scenario will be added to the Givens of this scenario
        /// </summary>
        /// <param name="givenScenario">A scenario that passes (this is important to not create an invalid given state)</param>
        /// <returns></returns>
        IEventGeneratingScenarioGivenStateBuilder Given(IEventCentricTestSpecificationBuilder givenScenario);

    }

    public interface IEventGeneratingScenarioGivenStateBuilder : IScenarioGivenStateBuilder
    {
        //IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(Func<IPostprocessComposer<TEvent>, IPostprocessComposer<TEvent>> customize = null);

        /// <summary>
        /// Given an event of type TEvent occurred
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="identifier">The aggregate identifier the events is to be associated with.</param>
        /// <param name="customize">An action that perfoms customizations of the event.</param>
        /// <returns></returns>
        IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<TEvent, TEvent> customize = null);

        /// <summary>
        /// Given an already tested scenario. The Givens and Thens of the specified scenario will be added to the Givens of this scenario
        /// </summary>
        /// <param name="givenScenario">A scenario that passes (this is important to not create an invalid given state)</param>
        /// <returns></returns>
        IEventGeneratingScenarioGivenStateBuilder Given(IEventCentricTestSpecificationBuilder givenScenario);
    }

    public class AutoAutoFixtureTestBuilder : IEventGeneratingScenarioGivenStateBuilder
    {
        private readonly IFixture _fixture;
        private readonly IScenarioGivenStateBuilder _inner;

        public AutoAutoFixtureTestBuilder(IScenarioGivenStateBuilder inner, IFixture fixture)
        {
            _inner = inner;
            _fixture = fixture;
        }
        public IScenarioGivenStateBuilder Given(params Fact[] facts)
        {
            return _inner.Given(facts);
        }

        public IScenarioGivenStateBuilder Given(string identifier, params object[] events)
        {
            return _inner.Given(identifier, events);
        }

        public IScenarioWhenStateBuilder When(object message)
        {
            return _inner.When(message);
        }

        public IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<IPostprocessComposer<TEvent>, IPostprocessComposer<TEvent>> customize = null)
        {
            IPostprocessComposer<TEvent> composer = _fixture.Build<TEvent>();
            if (customize != null)
                composer = customize(composer);

            var @event = composer.Create();
            return new AutoAutoFixtureTestBuilder(Given(identifier, @event), _fixture);
        }

        public IEventGeneratingScenarioGivenStateBuilder Given<TEvent>(string identifier, Func<TEvent, TEvent> customize = null)
        {
            var @event = _fixture.Create<TEvent>();
            if (customize != null)
                @event = customize(@event);

            return new AutoAutoFixtureTestBuilder(Given(identifier, @event), _fixture);
        }

        public IEventGeneratingScenarioGivenStateBuilder Given(IEventCentricTestSpecificationBuilder givenScenario)
        {
            var specification = givenScenario.Build();
            return new AutoAutoFixtureTestBuilder(this.Given(specification.Givens).Given(SetProvenance(specification.Thens)), _fixture);
        }

        private Fact[] SetProvenance(Fact[] facts)
        {
            foreach (var @event in facts)
            {
                if (@event.Event is ISetProvenance)
                {
                    var hasProvenance = @event.Event as IHasProvenance;
                    var setProvenance = @event.Event as ISetProvenance;
                    if (hasProvenance?.Provenance == null)
                        setProvenance.SetProvenance(_fixture.Create<Provenance>());
                }
            }

            return facts;
        }
    }
}
