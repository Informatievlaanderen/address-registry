namespace AddressRegistry.Tests.BackOffice.Validators
{
    using System.Security.Claims;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Moq;
    using Xunit;

    public class HouseNumberValidatorTests
    {
        [Theory]
        [InlineData("1", true)]
        [InlineData("1A", true)]
        [InlineData("123456789A", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("1234567890123A", false)]
        [InlineData("123Q", false)]
        public void ValidateWhenUserIsDecentraleBijwerker(string houseNumber, bool expectedResult)
        {
            var decentraleBijwerkerScope = Scopes.DvArAdresBeheer;
            var houseNumberValidator = SetupHouseNumberValidator(decentraleBijwerkerScope);

            houseNumberValidator.Validate(houseNumber).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("1A", true)]
        [InlineData("123456789A", true)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("1234567890123A", false)]
        public void ValidateWhenUserIsInterneBijwerker(string houseNumber, bool expectedResult)
        {
            var interneBijwerkerScope = Scopes.DvArAdresUitzonderingen;
            var houseNumberValidator = SetupHouseNumberValidator(interneBijwerkerScope);

            houseNumberValidator.Validate(houseNumber).Should().Be(expectedResult);
        }

        private static HouseNumberValidator SetupHouseNumberValidator(string scope)
        {
            DefaultHttpContext httpContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(AcmIdmClaimTypes.Scope, scope)
                    }))
            };

            var actionContextAccessor = new Mock<IActionContextAccessor>();
            actionContextAccessor
                .Setup(x => x.ActionContext)
                .Returns(new ActionContext
                {
                    HttpContext = httpContext
                });

            return new HouseNumberValidator(actionContextAccessor.Object);
        }
    }
}
