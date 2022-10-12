namespace AddressRegistry.Tests.AutoFixture;
using global::AutoFixture;
using global::AutoFixture.Kernel;
using StreetName;

public class WithFixedAddressPersistentLocalId : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var persistentLocalId = fixture.Create<int>() % (int.MaxValue - 1000 + 1) + 1000;
        var addressPersistentLocalId = new AddressPersistentLocalId(persistentLocalId);

        fixture.Register(() => addressPersistentLocalId);

        fixture.Customizations.Add(
            new FilteringSpecimenBuilder(
                new FixedBuilder(persistentLocalId),
                new ParameterSpecification(
                    typeof(int),
                    "addressPersistentLocalId")));
    }
}
