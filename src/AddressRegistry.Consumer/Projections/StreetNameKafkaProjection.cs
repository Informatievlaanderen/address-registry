namespace AddressRegistry.Consumer.Projections
{
    using System;
    using System.Linq;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using NodaTime.Text;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;

    /// <summary>
    /// Here we handle the streetname events which impact the state of the streetname aggregate.
    /// </summary>
    public class StreetNameKafkaProjection : ConnectedProjection<CommandHandler>
    {
        private static Provenance FromProvenance(Contracts.Provenance provenance, Modification modification = Modification.Update) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).Value,
                Application.StreetNameRegistry,
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                modification,
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
                    FromProvenance(msg.Provenance, Modification.Insert)
                );
            }

            if (type == typeof(StreetNameWasProposedV2))
            {
                var msg = (StreetNameWasProposedV2)message;
                return new ImportStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    StreetNameStatus.Proposed,
                    FromProvenance(msg.Provenance, Modification.Insert)
                );
            }

            if (type == typeof(StreetNameWasProposedForMunicipalityMerger))
            {
                var msg = (StreetNameWasProposedForMunicipalityMerger)message;
                return new ImportStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    StreetNameStatus.Proposed,
                    FromProvenance(msg.Provenance, Modification.Insert)
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

            if (type == typeof(StreetNameWasRejectedBecauseOfMunicipalityMerger))
            {
                var msg = (StreetNameWasRejectedBecauseOfMunicipalityMerger)message;
                return new RejectStreetNameBecauseOfMunicipalityMerger(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    msg.NewPersistentLocalIds.Select(x => new StreetNamePersistentLocalId(x)),
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

            if (type == typeof(StreetNameWasRetiredBecauseOfMunicipalityMerger))
            {
                var msg = (StreetNameWasRetiredBecauseOfMunicipalityMerger)message;
                return new RetireStreetNameBecauseOfMunicipalityMerger(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    msg.NewPersistentLocalIds.Select(x => new StreetNamePersistentLocalId(x)),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(StreetNameWasRenamed))
            {
                var msg = (StreetNameWasRenamed)message;
                return new RenameStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    new StreetNamePersistentLocalId(msg.DestinationPersistentLocalId),
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

            if (type == typeof(StreetNameWasRemovedV2))
            {
                var msg = (StreetNameWasRemovedV2)message;
                return new RemoveStreetName(
                    new StreetNamePersistentLocalId(msg.PersistentLocalId),
                    FromProvenance(msg.Provenance, Modification.Delete));
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

            When<StreetNameWasProposedForMunicipalityMerger>(async (commandHandler, message, ct) =>
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

            When<StreetNameWasRejectedBecauseOfMunicipalityMerger>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasRenamed>(async (commandHandler, message, ct) =>
            {
                var renameStreetName = new RenameStreetName(
                    new StreetNamePersistentLocalId(message.PersistentLocalId),
                    new StreetNamePersistentLocalId(message.DestinationPersistentLocalId),
                    FromProvenance(message.Provenance));

                await commandHandler.Handle(renameStreetName, ct);

                var retireStreetNameBecauseOfRename = new RetireStreetNameBecauseOfRename(
                    new StreetNamePersistentLocalId(message.PersistentLocalId),
                    new StreetNamePersistentLocalId(message.DestinationPersistentLocalId),
                    FromProvenance(message.Provenance));

                await commandHandler.Handle(retireStreetNameBecauseOfRename, ct);
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

            When<StreetNameWasRetiredBecauseOfMunicipalityMerger>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasCorrectedFromRetiredToCurrent>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<StreetNameWasRemovedV2>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });
        }
    }
}
