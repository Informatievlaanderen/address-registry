namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    public interface IIdempotentCommandHandler
    {
        Task<long> Dispatch(
            Guid? commandId,
            object command,
            IDictionary<string, object> metadata,
            CancellationToken cancellationToken);
    }
}
