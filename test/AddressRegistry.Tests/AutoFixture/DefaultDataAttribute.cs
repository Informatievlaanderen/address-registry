namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using global::AutoFixture;
    using global::AutoFixture.Xunit2;

    public class DefaultDataAttribute : AutoDataAttribute
    {
        public DefaultDataAttribute() : this(() => new Fixture()) { }

        protected DefaultDataAttribute(Func<IFixture> fixtureFactory)
            : base(() => fixtureFactory().Customize(new WithDefaults())) { }
    }

    public class DefaultDataForSubaddressAttribute : AutoDataAttribute
    {
        public DefaultDataForSubaddressAttribute() : this(() => new Fixture()) { }

        protected DefaultDataForSubaddressAttribute(Func<IFixture> fixtureFactory)
            : base(() => fixtureFactory().Customize(new WithDefaults(true))) { }
    }

    public class InlineDefaultDataAttribute : InlineAutoDataAttribute
    {
        public InlineDefaultDataAttribute(params object[] values) : base(new DefaultDataAttribute(), values)
        {

        }
    }

    public class InlineDefaultDataForSubaddressAttribute : InlineAutoDataAttribute
    {
        public InlineDefaultDataForSubaddressAttribute(params object[] values) : base(new DefaultDataForSubaddressAttribute(), values)
        {

        }
    }
}
