namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using global::AutoFixture.Kernel;

    public class WithUniqueInteger : ISpecimenBuilder
    {
        private int _lastInt;

        public object Create(object request, ISpecimenContext context)
        {
            if (request is not Type type || type != typeof(int))
            {
                return new NoSpecimen();
            }

            var nextInt = _lastInt + 1;
            _lastInt = nextInt;

            return nextInt;
        }
    }
}
