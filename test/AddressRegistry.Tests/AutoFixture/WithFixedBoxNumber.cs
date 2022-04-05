namespace AddressRegistry.Tests.AutoFixture
{
    using Address.ValueObjects;
    using global::AutoFixture;

    public class WithFixedBoxNumber : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var boxNumber = fixture.Create<BoxNumber>();
            fixture.Register(() => boxNumber);
        }
    }
}
