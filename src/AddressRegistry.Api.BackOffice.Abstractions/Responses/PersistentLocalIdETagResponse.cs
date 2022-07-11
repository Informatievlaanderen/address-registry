namespace AddressRegistry.Api.BackOffice.Abstractions.Responses
{
    public record PersistentLocalIdETagResponse(int PersistentLocalId, string LastEventHash);
}
