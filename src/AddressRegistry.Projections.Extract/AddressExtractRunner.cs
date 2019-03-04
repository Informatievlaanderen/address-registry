namespace AddressRegistry.Projections.Extract
{
    using AddressExtract;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.IO;

    public class AddressExtractRunner : Runner<ExtractContext>
    {
        public const string Name = "AddressExtractRunner";

        public AddressExtractRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<AddressExtractRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new AddressExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding(), new WKBReader())) // TODO: additional config needed for WKBReader?
        { }
    }
}
