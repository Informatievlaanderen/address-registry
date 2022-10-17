namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;

    public static class SqsLambdaRequestExtensions
    {
        public static StreetNamePersistentLocalId StreetNamePersistentLocalId(this SqsLambdaRequest request) =>
            new StreetNamePersistentLocalId(Convert.ToInt32(request.MessageGroupId));
    }
}
