namespace AddressRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;

    public class AddressLastChangedListRunner : LastChangedListRunner
    {
        public const string Name = "AddressLastChangedListRunner";

        public AddressLastChangedListRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<AddressLastChangedListRunner> logger) :
            base(
                Name,
                new Projections(),
                envelopeFactory,
                logger) { }
    }
}
