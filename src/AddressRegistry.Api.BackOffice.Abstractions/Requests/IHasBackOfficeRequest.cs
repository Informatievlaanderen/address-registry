namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    public interface IHasBackOfficeRequest<TBackOfficeRequest>
    {
        public TBackOfficeRequest Request { get; init; }
    }
}
