namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api.Extensions
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public static class ActionContextExtensions
    {
        public static LastObservedPosition GetLastObservedPositionFromRequest(this ActionContext context, LastObservedPosition position)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new LastObservedPosition(context.HttpContext.Request);
        }
    }
}
