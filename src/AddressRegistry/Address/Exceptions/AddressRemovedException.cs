namespace AddressRegistry.Address.Exceptions
{
    using System;

    public class AddressRemovedException : AddressRegistryException
    {
        public AddressRemovedException() { }

        public AddressRemovedException(string message) : base(message) { }

        public AddressRemovedException(string message, Exception inner) : base(message, inner) { }
    }
}
