namespace AddressRegistry
{
    using System;

    public class AddressRemovedException : AddressRegistryException
    {
        public AddressRemovedException() { }

        public AddressRemovedException(string message) : base(message) { }

        public AddressRemovedException(string message, Exception inner) : base(message, inner) { }
    }
}
