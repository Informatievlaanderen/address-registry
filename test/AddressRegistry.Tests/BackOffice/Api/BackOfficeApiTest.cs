namespace AddressRegistry.Tests.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AddressRegistry.Api.BackOffice.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using StreetName;
    using Tests;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance.AcmIdm;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Moq;
    using StreetName.Exceptions;
    using Xunit.Abstractions;

    public class BackOfficeApiTest : AddressRegistryTest
    {
        protected const string PublicTicketUrl = "https://www.ticketing.com";
        protected const string InternalTicketUrl = "https://www.internalticketing.com";

        protected IOptions<TicketingOptions> TicketingOptions { get; }
        protected Mock<IMediator> MockMediator { get; }
        protected Mock<IActionContextAccessor> MockActionContext { get; set; }

        protected BackOfficeApiTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            TicketingOptions = Options.Create(Fixture.Create<TicketingOptions>());
            TicketingOptions.Value.PublicBaseUrl = PublicTicketUrl;
            TicketingOptions.Value.InternalBaseUrl = InternalTicketUrl;

            MockMediator = new Mock<IMediator>();

            MockActionContext = new Mock<IActionContextAccessor>();
            MockActionContext.SetupProperty(x => x.ActionContext, new ActionContext{ HttpContext = new DefaultHttpContext()});
        }

        protected Uri CreateTicketUri(Guid ticketId)
        {
            return new Uri($"{InternalTicketUrl}/tickets/{ticketId:D}");
        }

        protected TApiController CreateApiBusControllerWithUser<TApiController>() where TApiController : ApiController
        {
            var controller = Activator.CreateInstance(
                typeof(TApiController),
                MockMediator.Object,
                TicketingOptions,
                MockActionContext.Object,
                new AcmIdmProvenanceFactory(Application.AddressRegistry, MockActionContext.Object)) as TApiController;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "username"),
                new(ClaimTypes.NameIdentifier, "userId"),
                new("name", "John Doe"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            if (controller == null)
            {
                throw new Exception("Could not find controller type");
            }

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            return controller;
        }

        protected IIfMatchHeaderValidator MockIfMatchValidator(bool expectedResult)
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();

            mockIfMatchHeaderValidator
                .Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<AddressPersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));

            return mockIfMatchHeaderValidator.Object;
        }

        protected IIfMatchHeaderValidator MockIfMatchValidatorThrowsAddressIsNotFoundException()
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();

            mockIfMatchHeaderValidator
                .Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<AddressPersistentLocalId>(), CancellationToken.None))
                .Throws<AddressIsNotFoundException>();

            return mockIfMatchHeaderValidator.Object;
        }

        protected IIfMatchHeaderValidator MockIfMatchValidatorThrowsAggregateNotFoundException()
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();

            mockIfMatchHeaderValidator
                .Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<AddressPersistentLocalId>(), CancellationToken.None))
                .Throws(() => new AggregateNotFoundException("test", typeof(StreetName)));

            return mockIfMatchHeaderValidator.Object;
        }

        protected IValidator<TRequest> MockValidRequestValidator<TRequest>()
        {
            var mockRequestValidator = new Mock<IValidator<TRequest>>();

            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            return mockRequestValidator.Object;
        }

        protected void AssertLocation(string? location, Guid ticketId)
        {
            var expectedLocation = $"{PublicTicketUrl}/tickets/{ticketId:D}";

            location.Should().NotBeNullOrWhiteSpace();
            location.Should().Be(expectedLocation);
        }
    }
}
