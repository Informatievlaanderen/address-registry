namespace AddressRegistry.Tests.AutoFixture
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;

    public class WithNoDeleteModification : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var crabModification = fixture.Create<Generator<CrabModification>>()
                .FirstOrDefault(modification => modification != CrabModification.Delete);

            fixture.Register(() => crabModification);
        }
    }
}
