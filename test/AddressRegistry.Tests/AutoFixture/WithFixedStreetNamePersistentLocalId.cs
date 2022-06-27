namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    public class WithFixedStreetNamePersistentLocalId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var persistentLocalId = fixture.Create<int>();
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(persistentLocalId);

            fixture.Register(() => streetNamePersistentLocalId);
            fixture.Register(() => new StreetNameStreamId(streetNamePersistentLocalId));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(persistentLocalId),
                    new ParameterSpecification(
                        typeof(int),
                        "streetNamePersistentLocalId")));
        }
    }
}
