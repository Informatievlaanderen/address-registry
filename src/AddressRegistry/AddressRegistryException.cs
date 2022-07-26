namespace AddressRegistry
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public abstract class AddressRegistryException : DomainException
    {
        protected AddressRegistryException()
        { }

        protected AddressRegistryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        
        protected AddressRegistryException(string message)
            : base(message)
        { }

        protected AddressRegistryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
