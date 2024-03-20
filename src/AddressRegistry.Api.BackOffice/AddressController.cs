namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "adressen")]
    public partial class AddressController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly TicketingOptions _ticketingOptions;

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IProvenanceFactory _provenanceFactory;

        public AddressController(
            IMediator mediator,
            IOptions<TicketingOptions> ticketingOptions,
            IActionContextAccessor actionContextAccessor,
            IProvenanceFactory provenanceFactory)
        {
            _mediator = mediator;
            _actionContextAccessor = actionContextAccessor;
            _provenanceFactory = provenanceFactory;
            _ticketingOptions = ticketingOptions.Value;
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

        public IActionResult Accepted(LocationResult locationResult)
        {
            return Accepted(locationResult
                .Location
                .ToString()
                .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl));
        }

        private IDictionary<string, object?> GetMetadata()
        {
            var correlationId = _actionContextAccessor
                .ActionContext?
                .HttpContext
                .Request
                .Headers["x-correlation-id"].FirstOrDefault() ?? Guid.NewGuid().ToString("D");

            return new Dictionary<string, object?>
            {
                { "CorrelationId", correlationId }
            };
        }

        protected Provenance CreateProvenance(Modification modification)
            => _provenanceFactory.Create(new Reason(""), modification);
    }
}
