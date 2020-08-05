namespace AddressRegistry.Api.Backoffice.Infrastructure
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using CrabEdit.Client;
    using Microsoft.AspNetCore.Http;

    public class CrabClientValidationExceptionHandler : DefaultExceptionHandler<CrabClientValidationException>
    {
        protected override ProblemDetails GetApiProblemFor(CrabClientValidationException exception)
        {
            var validationErrors = exception
                .ValidationErrors
                .ToDictionary(
                    key => key.ValidationErrorType,
                    value => new []{ value.ValidationMessage });

            return new ValidationProblemDetails
            {
                HttpStatus = StatusCodes.Status400BadRequest,
                Title = ProblemDetails.DefaultTitle,
                ProblemTypeUri = ProblemDetails.GetTypeUriFor(exception),
                Detail = "Fout bij de bron.",
                ValidationErrors = validationErrors
            };
        }
    }
}
