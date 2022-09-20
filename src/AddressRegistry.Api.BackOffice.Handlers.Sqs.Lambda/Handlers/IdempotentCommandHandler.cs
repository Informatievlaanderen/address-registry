namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Security.Cryptography;
    using System.Text;
    using Abstractions.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class IdempotentCommandHandler : IIdempotentCommandHandler
    {
        private readonly ICommandHandlerResolver _bus;
        private readonly IdempotencyContext _idempotencyContext;

        public IdempotentCommandHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext)
        {
            _bus = bus;
            _idempotencyContext = idempotencyContext;
        }

        public async Task<long> Dispatch(
            Guid? commandId,
            object command,
            IDictionary<string, object> metadata,
            CancellationToken cancellationToken)
        {
            if (!commandId.HasValue || command == null)
                throw new ApiException("Ongeldig verzoek id.", StatusCodes.Status400BadRequest);

            // First check if the command id already has been processed
            var possibleProcessedCommand = await _idempotencyContext
                .ProcessedCommands
                .Where(x => x.CommandId == commandId)
                .ToDictionaryAsync(x => x.CommandContentHash, x => x, cancellationToken);

            var contentHash = SHA512
                .Create()
                .ComputeHash(Encoding.UTF8.GetBytes(command.ToString()))
                .ToHexString();

            // It is possible we have a GUID collision, check the SHA-512 hash as well to see if it is really the same one.
            // Do nothing if commandId with contenthash exists
            if (possibleProcessedCommand.Any() && possibleProcessedCommand.ContainsKey(contentHash))
                throw new IdempotencyException("Already processed");

            var processedCommand = new ProcessedCommand(commandId.Value, contentHash);
            try
            {
                // Store commandId in Command Store if it does not exist
                await _idempotencyContext.ProcessedCommands.AddAsync(processedCommand, cancellationToken);
                await _idempotencyContext.SaveChangesAsync(cancellationToken);

                // Do work
                return await CommandHandlerResolverExtensions.Dispatch(
                    _bus,
                    commandId.Value,
                    command,
                    metadata,
                    cancellationToken);
            }
            catch
            {
                // On exception, remove commandId from Command Store
                _idempotencyContext.ProcessedCommands.Remove(processedCommand);
                _idempotencyContext.SaveChanges();
                throw;
            }
        }
    }
}
