namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api
{
    using Microsoft.AspNetCore.Http;

    public class CreatedResultWithLastObservedPosition : LastObservedPositionResult
    {
        public CreatedResultWithLastObservedPosition(string location, LastObservedPosition lastObservedPosition)
            : base(StatusCodes.Status201Created, location, lastObservedPosition) { }
    }
}
