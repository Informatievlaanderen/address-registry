namespace AddressRegistry.Api.Oslo.AddressMatch.Requests
{
    using MediatR;
    using Responses;

    public class AddressMatchRequest : IRequest<AddressMatchOsloCollection>
    {
        /// <summary>
        /// municipality name
        /// </summary>
        public string Gemeentenaam { get; set; }

        /// <summary>
        /// municipality code as known by the National Statistics Institute
        /// </summary>
        public string Niscode { get; set; }

        /// <summary>
        /// postal code
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// street name
        /// </summary>
        public string Straatnaam { get; set; }

        /// <summary>
        /// house number
        /// </summary>
        public string Huisnummer { get; set; }

        /// <summary>
        /// bus number to be interpreted litterally
        /// </summary>
        public string Busnummer { get; set; }
    }
}
