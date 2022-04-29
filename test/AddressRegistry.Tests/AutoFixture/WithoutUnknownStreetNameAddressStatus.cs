namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithoutUnknownStreetNameAddressStatus : ISpecimenBuilder
    {
        private readonly EnumGenerator _builder = new EnumGenerator();

        public object Create(object request, ISpecimenContext context)
        {
            var value = _builder.Create(request, context);
            if (value is StreetName.AddressStatus @enum && @enum == StreetName.AddressStatus.Unknown)
            {
                return _builder.Create(request, context);
            }

            return value;
        }
    }
}
