namespace AddressRegistry.Api.Legacy.AddressMatch.Requests
{
    using MediatR;
    using Responses;

    public class AddressMatchRequest : IRequest<AddressMatchCollection>
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
        /// street code as known by the Patrimonium Service
        /// </summary>
        public string KadStraatcode { get; set; }

        /// <summary>
        /// street code as known by the National Register
        /// </summary>
        public string RrStraatcode { get; set; }

        /// <summary>
        /// street name
        /// </summary>
        public string Straatnaam { get; set; }

        /// <summary>
        /// house number
        /// </summary>
        public string Huisnummer { get; set; }

        /// <summary>
        /// house number addition as known by the National Register
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// bus number to be interpreted litterally
        /// </summary>
        public string Busnummer { get; set; }
    }
}
