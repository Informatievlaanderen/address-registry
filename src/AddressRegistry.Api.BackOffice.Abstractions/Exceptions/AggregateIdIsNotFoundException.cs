namespace AddressRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AggregateIdIsNotFoundException : Exception
    {
        public AggregateIdIsNotFoundException()
        { }

        public AggregateIdIsNotFoundException(string? message)
            : base(message)
        { }

        private AggregateIdIsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
