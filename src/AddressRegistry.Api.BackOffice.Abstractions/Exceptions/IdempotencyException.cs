namespace AddressRegistry.Api.BackOffice.Abstractions.Exceptions
{
    using System;

    public class IdempotencyException : Exception
    {
        public IdempotencyException(string? message) : base(message)
        {
        }
    }
}
