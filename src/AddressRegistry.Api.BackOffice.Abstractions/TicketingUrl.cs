namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using System;
    using TicketingService.Abstractions;

    public class TicketingUrl : ITicketingUrl
    {
        public string Scheme { get; }
        public string Host { get; }
        public string PathBase { get; }

        public TicketingUrl(string scheme, string host, string pathBase)
        {
            Scheme = scheme;
            Host = host;
            PathBase = pathBase;
        }

        public string For(Guid ticketId) => $"{Scheme}://{Host}{PathBase}/{ticketId:D}";
    }
}
