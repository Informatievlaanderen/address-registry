namespace AddressRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressLdesExtensions
    {
        public static async Task FindAndUpdateAddressDetail(
            this ProducerContext context,
            int persistentLocalId,
            Action<AddressDetail> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .Addresses
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(persistentLocalId.ToString());
            }

            updateFunc(streetName);
        }
    }
}
