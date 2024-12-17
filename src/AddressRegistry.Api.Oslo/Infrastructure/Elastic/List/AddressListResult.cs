namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.List
{
    using System.Collections.Generic;
    using AddressRegistry.Projections.Elastic.AddressList;

    public sealed class AddressListResult
    {
        public IReadOnlyCollection<AddressListDocument> Addresses { get; }
        public long Total { get; }

        public AddressListResult(IReadOnlyCollection<AddressListDocument> addresses, long total)
        {
            Addresses = addresses;
            Total = total;
        }

        public static AddressListResult Empty => new AddressListResult(new List<AddressListDocument>(), 0);
    }
}
