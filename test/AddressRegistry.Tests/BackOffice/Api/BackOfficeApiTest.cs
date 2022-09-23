namespace AddressRegistry.Tests.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AddressRegistry.Api.BackOffice.Infrastructure.Options;
    using StreetName;
    using Tests;
    using Be.Vlaanderen.Basisregisters.Api;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Xunit.Abstractions;

    public class BackOfficeApiTest : AddressRegistryTest
    {
        private const string DetailUrl = "https://www.registry.com/address/voorgesteld/{0}";
        protected const string StraatNaamPuri = $"https://data.vlaanderen.be/id/straatnaam/";
        protected const string PostInfoPuri = $"https://data.vlaanderen.be/id/postinfo/";

        protected IOptions<ResponseOptions> ResponseOptions { get; }
        protected Mock<IMediator> MockMediator { get; }

        protected BackOfficeApiTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            ResponseOptions = Options.Create(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.DetailUrl = DetailUrl;
            MockMediator = new Mock<IMediator>();
        }

        protected T CreateApiBusControllerWithUser<T>(bool useSqs = false) where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object, new UseSqsToggle(useSqs)) as T;

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
                .Setup(x => x.IsValid(
                    It.IsAny<string>(), It.IsAny<StreetNamePersistentLocalId>(), It.IsAny<AddressPersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));

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
    }
}
