namespace AddressRegistry.Api.BackOffice
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "adressen")]
    public partial class AddressController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly UseSqsToggle _useSqsToggle;

        public AddressController(IMediator mediator, UseSqsToggle useSqsToggle)
        {
            _mediator = mediator;
            _useSqsToggle = useSqsToggle;
        }

        private ValidationException CreateValidationException(string errorCode, string propertyName, string message)
        {
            var failure = new ValidationFailure(propertyName, message)
            {
                ErrorCode = errorCode
            };

            return new ValidationException(new List<ValidationFailure>
            {
                failure
            });
        }

        private IDictionary<string, object?> GetMetadata()
        {
            var userId = User.FindFirst("urn:be:vlaanderen:addressregistry:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object?>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId }
            };
        }

        private Provenance CreateFakeProvenance()
        {
            return new Provenance(
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                Application.AddressRegistry,
                new Reason(""), // TODO: TBD
                new Operator(""), // TODO: from claims
                Modification.Insert,
                Organisation.DigitaalVlaanderen // TODO: from claims
            );
        }
    }
}
