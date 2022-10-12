namespace AddressRegistry.Consumer.Projections
{
    using System;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using NodaTime.Text;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Provenance;
    using StreetNameId = AddressRegistry.StreetName.StreetNameId;

    public class StreetNameKafkaProjection : ConnectedProjection<CommandHandler>
    {
        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).GetValueOrThrow(),
                Enum.Parse<Application>(provenance.Application),
                new Reason(provenance.Reason),
                new Operator(string.Empty), // TODO: municipality registry?
                Enum.Parse<Modification>(provenance.Modification),
                Enum.Parse<Organisation>(provenance.Organisation));

        public static IHasCommandProvenance GetCommand(IQueueMessage message)
        {
            var type = message.GetType();

            if (type == typeof(StreetNameWasMigratedToMunicipality))
            {
                var msg = (StreetNameWasMigratedToMunicipality)message;
                return new ImportMigratedStreetName(
                    StreetNameId.CreateFor(msg.StreetNameId),
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    new NisCode(msg.NisCode),
                    Enum.Parse<StreetNameStatus>(msg.Status),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasProposedV2))
            {
                var msg = (StreetNameWasProposedV2)message;
                return new ImportStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    StreetNameStatus.Proposed,
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasApproved))
            {
                var msg = (StreetNameWasApproved)message;
                return new ApproveStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasCorrectedFromApprovedToProposed))
            {
                var msg = (StreetNameWasCorrectedFromApprovedToProposed)message;
                return new CorrectStreetNameApproval(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasCorrectedFromRejectedToProposed))
            {
                var msg = (StreetNameWasCorrectedFromRejectedToProposed)message;
                return new CorrectStreetNameRejection(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasRejected))
            {
                var msg = (StreetNameWasRejected)message;
                return new RejectStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasRetiredV2))
            {
                var msg = (StreetNameWasRetiredV2)message;
                return new RetireStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasCorrectedFromRetiredToCurrent))
            {
                var msg = (StreetNameWasCorrectedFromRetiredToCurrent)message;
                return new CorrectStreetNameRetirement(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance)
                );
            }

            throw new InvalidOperationException($"No command found for {type.FullName}");
        }

        public StreetNameKafkaProjection()
        {
            When<StreetNameWasMigratedToMunicipality>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);

                if (message.IsRemoved)
                {
                    await commandHandler.Handle(
                        new RemoveStreetName(
                            new StreetNamePersistentLocalId(message.PersistentLocalId),
                            FromProvenance(message.Provenance)),
                        ct);
                }
            });

            When<StreetNameWasProposedV2>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasApproved>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasCorrectedFromApprovedToProposed>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasRejected>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasCorrectedFromRejectedToProposed>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasRetiredV2>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasCorrectedFromRetiredToCurrent>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });
        }
    }
}
