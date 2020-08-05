namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api
{
    using Microsoft.AspNetCore.Http;

    public class AcceptedResultWithLastObservedPosition : LastObservedPositionResult
    {
        public AcceptedResultWithLastObservedPosition(LastObservedPosition lastObservedPosition)
            : base(StatusCodes.Status202Accepted, lastObservedPosition) { }
    }
}
