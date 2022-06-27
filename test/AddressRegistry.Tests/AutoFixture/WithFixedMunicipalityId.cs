namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    public class WithFixedMunicipalityId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var municipalityId = fixture.Create<MunicipalityId>();
            fixture.Register(() => municipalityId);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(municipalityId),
                    new ParameterSpecification(
                        typeof(int),
                        "municipalityId")));
        }
    }
}
