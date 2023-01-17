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
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).GetValueOrThrow(),
                Application.StreetNameRegistry,
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                Enum.Parse<Modification>(Modification.Update.ToString()),
                Enum.Parse<Organisation>(Organisation.DigitaalVlaanderen.ToString()));
    }
}
