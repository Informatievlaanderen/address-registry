namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using System;

    [Serializable]
    public abstract class AddressRegistryException : DomainException
    {
        protected AddressRegistryException()
        { }

        protected AddressRegistryException(string message)
            : base(message)
        { }

        protected AddressRegistryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
