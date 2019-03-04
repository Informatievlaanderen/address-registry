namespace AddressRegistry.Importer.Subaddress
{
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Xunit;

    public class UnitTestConfigForSubAddress : IUnitTestGeneratorConfig
    {
        public string BasePath => @"..\..\..\..\test\AddressRegistry.Tests\Bugfixes";
        public string NamespaceName => "AddressRegistry.Tests.Bugfixes";
        public string ClassNamePrefix => "TestSubaddress";
        public string BaseClassName => "AddressRegistryTest";
        public string GetClassName(object key)
        {
            return $"{ClassNamePrefix}_{key}";
        }

        public string GetCreateIdStatement(object key)
        {
            return $"new AddressId(new CrabSubaddressId({key}).CreateDeterministicId())";
        }
    }
}
