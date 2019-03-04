namespace AddressRegistry.Importer.HouseNumber
{
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Xunit;

    public class UnitTestConfigForHuisnummer : IUnitTestGeneratorConfig
    {
        public string BasePath => @"..\..\..\..\test\AddressRegistry.Tests\Bugfixes";

        public string NamespaceName => "AddressRegistry.Tests.Bugfixes";

        public string ClassNamePrefix => "TestHouseNumber";

        public string BaseClassName => "AddressRegistryTest";

        public string GetClassName(object key) => $"{ClassNamePrefix}_{key}";

        public string GetCreateIdStatement(object key) => $"new AddressId(new CrabHouseNumberId({key}).CreateDeterministicId())";
    }
}
