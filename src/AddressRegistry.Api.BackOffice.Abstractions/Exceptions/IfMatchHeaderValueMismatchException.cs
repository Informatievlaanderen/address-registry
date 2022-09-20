namespace AddressRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class IfMatchHeaderValueMismatchException : Exception
    {
        public IfMatchHeaderValueMismatchException()
        { }

        public IfMatchHeaderValueMismatchException(string? message)
            : base(message)
        { }

        private IfMatchHeaderValueMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
