namespace AddressRegistry.Api.CrabImport.CrabImport
{
    using Address;
    using Address.Commands.Crab;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using SqlStreamStore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly AddressCommandHandlerModule _addressCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Address, Provenance> _provenanceFactory = new AddressProvenanceFactory().CreateFrom;
        private readonly Func<IAddresses> _getAddresses;

        public IdempotentCommandHandlerModuleProcessor(
            Func<IAddresses> getAddresses,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            IOsloIdGenerator osloIdGenerator,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            AddressProvenanceFactory provenanceFactory)
        {
            _getAddresses = getAddresses;
            _concurrentUnitOfWork = concurrentUnitOfWork;

            _addressCommandHandlerModule = new AddressCommandHandlerModule(
                _getAddresses,
                () => concurrentUnitOfWork,
                osloIdGenerator,
                getStreamStore,
                eventMapping,
                eventSerializer,
                provenanceFactory);
        }

        public async Task<CommandMessage> Process(
            dynamic commandToProcess,
            IDictionary<string, object> metadata,
            int currentPosition,
            CancellationToken cancellationToken)
        {
            switch (commandToProcess)
            {
                case ImportHouseNumberFromCrab command:
                    var commandHouseNumberMessage = new CommandMessage<ImportHouseNumberFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportHouseNumberFromCrab(_getAddresses, commandHouseNumberMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberMessage;

                case ImportHouseNumberStatusFromCrab command:
                    var commandHouseNumberStatusMessage = new CommandMessage<ImportHouseNumberStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportHouseNumberStatusFromCrab(_getAddresses, commandHouseNumberStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberStatusMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberStatusMessage;

                case ImportHouseNumberPositionFromCrab command:
                    var commandHouseNumberPositionMessage = new CommandMessage<ImportHouseNumberPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportHouseNumberPositionFromCrab(_getAddresses, commandHouseNumberPositionMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberPositionMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberPositionMessage;

                case ImportHouseNumberMailCantonFromCrab command:
                    var commandHouseNumberMailCantonMessage = new CommandMessage<ImportHouseNumberMailCantonFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportHouseNumberMailCantonFromCrab(_getAddresses, commandHouseNumberMailCantonMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberMailCantonMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberMailCantonMessage;

                case ImportHouseNumberSubaddressFromCrab command:
                    var commandHouseNumberSubaddressMessage = new CommandMessage<ImportHouseNumberSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportHouseNumberSubaddressFromCrab(_getAddresses, commandHouseNumberSubaddressMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberSubaddressMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberSubaddressMessage;

                case ImportSubaddressFromCrab command:
                    var commandSubaddressMessage = new CommandMessage<ImportSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportSubaddressFromCrab(_getAddresses, commandSubaddressMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressMessage;

                case ImportSubaddressStatusFromCrab command:
                    var commandSubaddressStatusMessage = new CommandMessage<ImportSubaddressStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportSubaddressStatusFromCrab(_getAddresses, commandSubaddressStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressStatusMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressStatusMessage;

                case ImportSubaddressPositionFromCrab command:
                    var commandSubaddressPositionMessage = new CommandMessage<ImportSubaddressPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportSubaddressPositionFromCrab(_getAddresses, commandSubaddressPositionMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressPositionMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressPositionMessage;

                case ImportSubaddressMailCantonFromCrab command:
                    var commandSubaddressMailCantonMessage = new CommandMessage<ImportSubaddressMailCantonFromCrab>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.ImportSubaddressMailCantonFromCrab(_getAddresses, commandSubaddressMailCantonMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressMailCantonMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressMailCantonMessage;

                case AssignOsloIdForCrabHouseNumberId command:
                    var commandAssignOsloIdForCrabHouseNumberId = new CommandMessage<AssignOsloIdForCrabHouseNumberId>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.AssignOsloIdForCrabHouseNumberId(_getAddresses, commandAssignOsloIdForCrabHouseNumberId, cancellationToken);
                    return commandAssignOsloIdForCrabHouseNumberId;

                case AssignOsloIdForCrabSubaddressId command:
                    var commandAssignOsloIdForCrabSubaddressId = new CommandMessage<AssignOsloIdForCrabSubaddressId>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.AssignOsloIdForCrabSubaddressId(_getAddresses, commandAssignOsloIdForCrabSubaddressId, cancellationToken);
                    return commandAssignOsloIdForCrabSubaddressId;

                case RequestOsloIdForCrabHouseNumberId command:
                    var commandRequestOsloIdForCrabHouseNumberId = new CommandMessage<RequestOsloIdForCrabHouseNumberId>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.RequestOsloIdForCrabHouseNumberId(_getAddresses, commandRequestOsloIdForCrabHouseNumberId, cancellationToken);
                    return commandRequestOsloIdForCrabHouseNumberId;

                case RequestOsloIdForCrabSubaddressId command:
                    var commandRequestOsloIdForCrabSubaddressId = new CommandMessage<RequestOsloIdForCrabSubaddressId>(command.CreateCommandId(), command, metadata);
                    await _addressCommandHandlerModule.RequestOsloIdForCrabSubaddressId(_getAddresses, commandRequestOsloIdForCrabSubaddressId, cancellationToken);
                    return commandRequestOsloIdForCrabSubaddressId;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
