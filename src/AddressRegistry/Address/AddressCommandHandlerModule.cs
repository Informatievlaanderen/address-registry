namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using Commands.Crab;
    using NodaTime;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using SqlStreamStore;

    public sealed class AddressCommandHandlerModule : CommandHandlerModule
    {
        private readonly IOsloIdGenerator _osloIdGenerator;

        public AddressCommandHandlerModule(
            Func<IAddresses> getAddresses,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            IOsloIdGenerator osloIdGenerator,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            AddressProvenanceFactory provenanceFactory)
        {
            _osloIdGenerator = osloIdGenerator;

            For<RegisterAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle((message, ct) =>
                {
                    var addressId = message.Command.AddressId;
                    var address = Address.Register(
                        addressId,
                        message.Command.StreetNameId,
                        message.Command.HouseNumber);

                    var addresses = getAddresses();
                    addresses.Add(addressId, address);

                    return Task.CompletedTask;
                });

            For<ImportHouseNumberFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportHouseNumberFromCrab(getAddresses, message, ct));

            For<ImportHouseNumberStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportHouseNumberStatusFromCrab(getAddresses, message, ct));

            For<ImportHouseNumberPositionFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportHouseNumberPositionFromCrab(getAddresses, message, ct));

            For<ImportHouseNumberMailCantonFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportHouseNumberMailCantonFromCrab(getAddresses, message, ct));

            For<ImportHouseNumberSubaddressFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportHouseNumberSubaddressFromCrab(getAddresses, message, ct));

            For<ImportSubaddressFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportSubaddressFromCrab(getAddresses, message, ct));

            For<ImportSubaddressStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportSubaddressStatusFromCrab(getAddresses, message, ct));

            For<ImportSubaddressPositionFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportSubaddressPositionFromCrab(getAddresses, message, ct));

            For<ImportSubaddressMailCantonFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await ImportSubaddressMailCantonFromCrab(getAddresses, message, ct));

            For<AssignOsloIdForCrabHouseNumberId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                //.AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await AssignOsloIdForCrabHouseNumberId(getAddresses, message, ct));

            For<AssignOsloIdForCrabSubaddressId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                //.AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await AssignOsloIdForCrabSubaddressId(getAddresses, message, ct));

            For<RequestOsloIdForCrabHouseNumberId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                //.AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await RequestOsloIdForCrabHouseNumberId(getAddresses, message, ct));

            For<RequestOsloIdForCrabSubaddressId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                //.AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await RequestOsloIdForCrabSubaddressId(getAddresses, message, ct));

            For<AssignOsloIdToAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => await AssignOsloIdToAddress(getAddresses, message, ct));
        }

        public async Task ImportSubaddressMailCantonFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportSubaddressMailCantonFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportSubaddressMailCantonFromCrab(address, message.Command);
        }

        public async Task ImportSubaddressPositionFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportSubaddressPositionFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportSubaddressPositionFromCrab(address, message.Command);
        }

        public async Task ImportSubaddressStatusFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportSubaddressStatusFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportSubaddressStatusFromCrab(address, message.Command);
        }

        public async Task ImportHouseNumberSubaddressFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportHouseNumberSubaddressFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var streetNameId = StreetNameId.CreateFor(message.Command.StreetNameId);
            var address = await GetOrRegisterAddress(addresses, addressId, streetNameId, message.Command.HouseNumber, ct);

            if (!address.HasValue)
            {
                address = new Optional<Address>(
                    Address.Register(
                        new AddressId(addressId),
                        new StreetNameId(streetNameId),
                        message.Command.HouseNumber));

                addresses.Add(addressId, address.Value);
            }

            ImportHouseNumberSubaddressFromCrab(address.Value, message.Command);
        }

        public async Task ImportSubaddressFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportSubaddressFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportSubaddressFromCrab(address, message.Command);
        }

        public async Task ImportHouseNumberMailCantonFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportHouseNumberMailCantonFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportHouseNumberMailCantonFromCrab(address, message.Command);
        }

        public async Task ImportHouseNumberPositionFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportHouseNumberPositionFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportHouseNumberPositionFromCrab(address, message.Command);
        }

        public async Task ImportHouseNumberStatusFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportHouseNumberStatusFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var address = await addresses.GetAsync(addressId, ct);

            ImportHouseNumberStatusFromCrab(address, message.Command);
        }

        public async Task AssignOsloIdForCrabHouseNumberId(Func<IAddresses> getAddresses, CommandMessage<AssignOsloIdForCrabHouseNumberId> message, CancellationToken ct)
        {
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var address = await getAddresses().GetAsync(addressId, ct);

            address.AssignOsloId(
                message.Command.OsloId,
                message.Command.AssignmentDate);
        }

        public async Task AssignOsloIdForCrabSubaddressId(Func<IAddresses> getAddresses, CommandMessage<AssignOsloIdForCrabSubaddressId> message, CancellationToken ct)
        {
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await getAddresses().GetAsync(addressId, ct);

            address.AssignOsloId(
                message.Command.OsloId,
                message.Command.AssignmentDate);
        }

        public async Task RequestOsloIdForCrabHouseNumberId(Func<IAddresses> getAddresses, CommandMessage<RequestOsloIdForCrabHouseNumberId> message, CancellationToken ct)
        {
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var address = await getAddresses().GetAsync(addressId, ct);

            address.RequestOsloId(_osloIdGenerator);
        }

        public async Task RequestOsloIdForCrabSubaddressId(Func<IAddresses> getAddresses, CommandMessage<RequestOsloIdForCrabSubaddressId> message, CancellationToken ct)
        {
            var addressId = AddressId.CreateFor(message.Command.SubaddressId);
            var address = await getAddresses().GetAsync(addressId, ct);

            address.RequestOsloId(_osloIdGenerator);
        }

        public async Task ImportHouseNumberFromCrab(Func<IAddresses> getAddresses, CommandMessage<ImportHouseNumberFromCrab> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var addressId = AddressId.CreateFor(message.Command.HouseNumberId);
            var streetNameId = StreetNameId.CreateFor(message.Command.StreetNameId);
            var address = await GetOrRegisterAddress(addresses, addressId, streetNameId, message.Command.HouseNumber, ct);

            if (!address.HasValue)
            {
                address = new Optional<Address>(
                    Address.Register(
                        new AddressId(addressId),
                        new StreetNameId(streetNameId),
                        message.Command.HouseNumber));

                addresses.Add(addressId, address.Value);
            }

            ImportHouseNumberFromCrab(address.Value, message.Command);
        }

        public async Task AssignOsloIdToAddress(Func<IAddresses> getAddresses, CommandMessage<AssignOsloIdToAddress> message, CancellationToken ct)
        {
            var addresses = getAddresses();
            var address = await addresses.GetAsync(message.Command.AddressId, ct);

            address.AssignOsloId(
                message.Command.OsloId,
                new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)));
        }

        private static void ImportSubaddressMailCantonFromCrab(Address address, ImportSubaddressMailCantonFromCrab message)
        {
            address.ImportHouseNumberMailCantonFromCrab(
                message.HouseNumberMailCantonId,
                message.HouseNumberId,
                message.MailCantonId,
                message.MailCantonCode,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportSubaddressPositionFromCrab(Address address, ImportSubaddressPositionFromCrab message)
        {
            address.ImportSubaddressPositionFromCrab(
                message.AddressPositionId,
                message.SubaddressId,
                message.AddressPosition,
                message.AddressPositionOrigin,
                message.AddressNature,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportSubaddressStatusFromCrab(Address address, ImportSubaddressStatusFromCrab message)
        {
            address.ImportSubaddressStatusFromCrab(
                message.SubaddressStatusId,
                message.SubaddressId,
                message.AddressStatus,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportHouseNumberSubaddressFromCrab(Address address, ImportHouseNumberSubaddressFromCrab message)
        {
            address.ImportHouseNumberFromCrab(
                message.HouseNumberId,
                message.StreetNameId,
                message.HouseNumber,
                message.GrbNotation,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportSubaddressFromCrab(Address address, ImportSubaddressFromCrab message)
        {
            address.ImportSubaddressFromCrab(
                message.SubaddressId,
                message.HouseNumberId,
                message.BoxNumber,
                message.BoxNumberType,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportHouseNumberMailCantonFromCrab(Address address, ImportHouseNumberMailCantonFromCrab message)
        {
            address.ImportHouseNumberMailCantonFromCrab(
                message.HouseNumberMailCantonId,
                message.HouseNumberId,
                message.MailCantonId,
                message.MailCantonCode,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportHouseNumberPositionFromCrab(Address address, ImportHouseNumberPositionFromCrab message)
        {
            address.ImportHouseNumberPositionFromCrab(
                message.AddressPositionId,
                message.HouseNumberId,
                message.AddressPosition,
                message.AddressPositionOrigin,
                message.AddressNature,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportHouseNumberStatusFromCrab(Address address, ImportHouseNumberStatusFromCrab message)
        {
            address.ImportHouseNumberStatusFromCrab(
                message.HouseNumberStatusId,
                message.HouseNumberId,
                message.AddressStatus,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static void ImportHouseNumberFromCrab(Address address, ImportHouseNumberFromCrab message)
        {
            address.ImportHouseNumberFromCrab(
                message.HouseNumberId,
                message.StreetNameId,
                message.HouseNumber,
                message.GrbNotation,
                message.Lifetime,
                message.Timestamp,
                message.Operator,
                message.Modification,
                message.Organisation);
        }

        private static async Task<Optional<Address>> GetOrRegisterAddress(
            IAddresses addresses,
            AddressId addressId,
            StreetNameId streetNameId,
            HouseNumber houseNumber,
            CancellationToken ct)
        {
            var address = await addresses.GetOptionalAsync(addressId, ct);

            if (address.HasValue)
                return address;

            address = new Optional<Address>(
                Address.Register(
                    addressId,
                    streetNameId,
                    houseNumber));

            addresses.Add(addressId, address.Value);

            return address;
        }
    }
}
