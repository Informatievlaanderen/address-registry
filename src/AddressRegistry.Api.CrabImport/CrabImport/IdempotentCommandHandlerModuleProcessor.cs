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
        private readonly CrabAddressCommandHandlerModule _crabAddressCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Address, Provenance> _provenanceFactory;
        private readonly Func<IAddresses> _getAddresses;
        private readonly Func<object, Address, Provenance> _addressPersistentLocalIdProvenanceFactory;

        public IdempotentCommandHandlerModuleProcessor(
            Func<IAddresses> getAddresses,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            AddressProvenanceFactory addressProvenanceFactory,
            CrabAddressProvenanceFactory crabProvenanceFactory,
            AddressLegacyProvenanceFactory legacyProvenanceFactory,
            AddressPersistentLocalIdentifierProvenanceFactory addressPersistentLocalIdentifierProvenanceFactory)
        {
            _getAddresses = getAddresses;
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _provenanceFactory = crabProvenanceFactory.CreateFrom;
            _addressPersistentLocalIdProvenanceFactory = addressPersistentLocalIdentifierProvenanceFactory.CreateFrom;

            _crabAddressCommandHandlerModule = new CrabAddressCommandHandlerModule(
                _getAddresses,
                () => concurrentUnitOfWork,
                persistentLocalIdGenerator,
                getStreamStore,
                eventMapping,
                eventSerializer,
                addressProvenanceFactory,
                crabProvenanceFactory,
                legacyProvenanceFactory,
                addressPersistentLocalIdentifierProvenanceFactory);
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
                    await _crabAddressCommandHandlerModule.ImportHouseNumberFromCrab(_getAddresses, commandHouseNumberMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberMessage;

                case ImportHouseNumberStatusFromCrab command:
                    var commandHouseNumberStatusMessage = new CommandMessage<ImportHouseNumberStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportHouseNumberStatusFromCrab(_getAddresses, commandHouseNumberStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberStatusMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberStatusMessage;

                case ImportHouseNumberPositionFromCrab command:
                    var commandHouseNumberPositionMessage = new CommandMessage<ImportHouseNumberPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportHouseNumberPositionFromCrab(_getAddresses, commandHouseNumberPositionMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberPositionMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberPositionMessage;

                case ImportHouseNumberMailCantonFromCrab command:
                    var commandHouseNumberMailCantonMessage = new CommandMessage<ImportHouseNumberMailCantonFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportHouseNumberMailCantonFromCrab(_getAddresses, commandHouseNumberMailCantonMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberMailCantonMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberMailCantonMessage;

                case ImportHouseNumberSubaddressFromCrab command:
                    var commandHouseNumberSubaddressMessage = new CommandMessage<ImportHouseNumberSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportHouseNumberSubaddressFromCrab(_getAddresses, commandHouseNumberSubaddressMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberSubaddressMessage, _provenanceFactory, currentPosition);
                    return commandHouseNumberSubaddressMessage;

                case ImportSubaddressFromCrab command:
                    var commandSubaddressMessage = new CommandMessage<ImportSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportSubaddressFromCrab(_getAddresses, commandSubaddressMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressMessage;

                case ImportSubaddressStatusFromCrab command:
                    var commandSubaddressStatusMessage = new CommandMessage<ImportSubaddressStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportSubaddressStatusFromCrab(_getAddresses, commandSubaddressStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressStatusMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressStatusMessage;

                case ImportSubaddressPositionFromCrab command:
                    var commandSubaddressPositionMessage = new CommandMessage<ImportSubaddressPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportSubaddressPositionFromCrab(_getAddresses, commandSubaddressPositionMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressPositionMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressPositionMessage;

                case ImportSubaddressMailCantonFromCrab command:
                    var commandSubaddressMailCantonMessage = new CommandMessage<ImportSubaddressMailCantonFromCrab>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.ImportSubaddressMailCantonFromCrab(_getAddresses, commandSubaddressMailCantonMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressMailCantonMessage, _provenanceFactory, currentPosition);
                    return commandSubaddressMailCantonMessage;

                case AssignPersistentLocalIdForCrabHouseNumberId command:
                    var commandAssignPersistentLocalIdForCrabHouseNumberId = new CommandMessage<AssignPersistentLocalIdForCrabHouseNumberId>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.AssignPersistentLocalIdForCrabHouseNumberId(_getAddresses, commandAssignPersistentLocalIdForCrabHouseNumberId, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandAssignPersistentLocalIdForCrabHouseNumberId, _addressPersistentLocalIdProvenanceFactory, currentPosition);
                    return commandAssignPersistentLocalIdForCrabHouseNumberId;

                case AssignPersistentLocalIdForCrabSubaddressId command:
                    var commandAssignPersistentLocalIdForCrabSubaddressId = new CommandMessage<AssignPersistentLocalIdForCrabSubaddressId>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.AssignPersistentLocalIdForCrabSubaddressId(_getAddresses, commandAssignPersistentLocalIdForCrabSubaddressId, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandAssignPersistentLocalIdForCrabSubaddressId, _addressPersistentLocalIdProvenanceFactory, currentPosition);
                    return commandAssignPersistentLocalIdForCrabSubaddressId;

                case RequestPersistentLocalIdForCrabHouseNumberId command:
                    var commandRequestPersistentLocalIdForCrabHouseNumberId = new CommandMessage<RequestPersistentLocalIdForCrabHouseNumberId>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.RequestPersistentLocalIdForCrabHouseNumberId(_getAddresses, commandRequestPersistentLocalIdForCrabHouseNumberId, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandRequestPersistentLocalIdForCrabHouseNumberId, _addressPersistentLocalIdProvenanceFactory, currentPosition);
                    return commandRequestPersistentLocalIdForCrabHouseNumberId;

                case RequestPersistentLocalIdForCrabSubaddressId command:
                    var commandRequestPersistentLocalIdForCrabSubaddressId = new CommandMessage<RequestPersistentLocalIdForCrabSubaddressId>(command.CreateCommandId(), command, metadata);
                    await _crabAddressCommandHandlerModule.RequestPersistentLocalIdForCrabSubaddressId(_getAddresses, commandRequestPersistentLocalIdForCrabSubaddressId, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandRequestPersistentLocalIdForCrabSubaddressId, _addressPersistentLocalIdProvenanceFactory, currentPosition);
                    return commandRequestPersistentLocalIdForCrabSubaddressId;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
