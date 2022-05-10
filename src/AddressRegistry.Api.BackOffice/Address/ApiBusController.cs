namespace AddressRegistry.Api.BackOffice.Address
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public abstract class ApiBusController : ApiController
    {
        protected ICommandHandlerResolver Bus { get; }
        protected ApiBusController(ICommandHandlerResolver bus) => Bus = bus;

        protected IDictionary<string, object> GetMetadata()
        {
            var userId = User.FindFirst("urn:be:vlaanderen:addressregistry:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId }
            };
        }

        protected async Task<long> IdempotentCommandHandlerDispatch(
            IdempotencyContext context,
            Guid? commandId,
            object command,
            CancellationToken cancellationToken)
        {
            if (!commandId.HasValue || command == null)
                throw new ApiException("Ongeldig verzoek id.", StatusCodes.Status400BadRequest);

            // First check if the command id already has been processed
            var possibleProcessedCommand = await context
                .ProcessedCommands
                .Where(x => x.CommandId == commandId)
                .ToDictionaryAsync(x => x.CommandContentHash, x => x, cancellationToken);


            var contentHash = SHA512
                .Create()
                .ComputeHash(Encoding.UTF8.GetBytes((string) command.ToString()))
                .ToHexString();

            // It is possible we have a GUID collision, check the SHA-512 hash as well to see if it is really the same one.
            // Do nothing if commandId with contenthash exists
            if (possibleProcessedCommand.Any() && possibleProcessedCommand.ContainsKey(contentHash))
                throw new IdempotencyException("Already processed");

            var processedCommand = new ProcessedCommand(commandId.Value, contentHash);

            try
            {
                // Store commandId in Command Store if it does not exist
                await context.ProcessedCommands.AddAsync(processedCommand, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                // Do work
                return await CommandHandlerResolverExtensions.Dispatch(
                    Bus,
                    commandId.Value,
                    command,
                    GetMetadata(),
                    cancellationToken);

            }
            catch
            {
                // On exception, remove commandId from Command Store
                context.ProcessedCommands.Remove(processedCommand);
                context.SaveChanges();
                throw;
            }
        }
    }

    public class IdempotencyException : Exception
    {
        public IdempotencyException(string? message) : base(message)
        {
        }
    }
}
