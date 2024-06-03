namespace AddressRegistry.Consumer.Read.StreetName.Projections
{
    using System;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using NodaTime.Text;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;

    /// <summary>
    /// Here we handle the streetname events which are not tracked in the domain.
    /// The reason we have an addition command handling here, is to keep the address version up-to-date,
    /// AND we don't want to have the StreetNameLatestItemProject ahead of this projection.
    /// </summary>
    public class StreetNameCommandHandlingProjections : ConnectedProjection<StreetNameCommandHandler>
    {
        public StreetNameCommandHandlingProjections()
        {
            When<StreetNameNamesWereCorrected>(async (commandHandler, message, ct) =>
            {
                var command = new CorrectStreetNameNames(
                    new StreetNamePersistentLocalId(message.PersistentLocalId),
                    message.StreetNameNames,
                    FromProvenance(message.Provenance));

                await commandHandler.Handle(command, ct);
            });

            When<StreetNameHomonymAdditionsWereCorrected>(async (commandHandler, message, ct) =>
            {
                var command = new CorrectStreetNameHomonymAdditions(
                    new StreetNamePersistentLocalId(message.PersistentLocalId),
                    message.HomonymAdditions,
                    FromProvenance(message.Provenance));

                await commandHandler.Handle(command, ct);
            });

            When<StreetNameHomonymAdditionsWereRemoved>(async (commandHandler, message, ct) =>
            {
                var command = new RemoveStreetNameHomonymAdditions(
                    new StreetNamePersistentLocalId(message.PersistentLocalId),
                    message.Languages,
                    FromProvenance(message.Provenance));

                await commandHandler.Handle(command, ct);
            });
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).Value,
                Application.StreetNameRegistry,
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                Enum.Parse<Modification>(Modification.Update.ToString()),
                Enum.Parse<Organisation>(Organisation.DigitaalVlaanderen.ToString()));
    }
}
