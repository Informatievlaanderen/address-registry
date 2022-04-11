namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using StreetName;

    public class WithoutParentAddressPersistentLocalId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var persistentLocalId = (int?)null;

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(persistentLocalId),
                    new ParameterSpecification(
                        typeof(int?),
                        "parentPersistentLocalId")));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder((AddressPersistentLocalId?)null),
                    new ParameterSpecification(
                        typeof(AddressPersistentLocalId),
                        "parentPersistentLocalId")));
        }
    }
}
