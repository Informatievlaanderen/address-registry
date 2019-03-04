namespace AddressRegistry.Projections.Legacy
{
    using AddressDetail;
    using AddressList;
    using AddressSyndication;
    using AddressVersion;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using CrabIdToOsloId;
    using Microsoft.Extensions.Logging;

    public class AddressLegacyRunner : Runner<LegacyContext>
    {
        public const string Name = "AddressLegacyRunner";

        public AddressLegacyRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<AddressLegacyRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new AddressListProjections(),
                new AddressDetailProjections(),
                new AddressVersionProjections(),
                new AddressSyndicationProjections(),
                new CrabIdToOsloIdProjections()) { }
    }
}
