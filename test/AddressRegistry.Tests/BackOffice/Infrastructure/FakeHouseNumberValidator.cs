namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System.Security.Claims;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using Moq;

    public static class FakeHouseNumberValidator
    {
        private static HouseNumberValidator? _interneBijwerkerInstance;
        private static HouseNumberValidator? _decentraleBijwerkerInstance;
        public static HouseNumberValidator InstanceInterneBijwerker => _interneBijwerkerInstance ??= Create(Scopes.DvArAdresUitzonderingen);
        public static HouseNumberValidator InstanceDecentraleBijwerker => _decentraleBijwerkerInstance ??= Create(Scopes.DvArAdresBeheer);

        private static HouseNumberValidator Create(string scope)
        {
            DefaultHttpContext httpContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(AcmIdmClaimTypes.Scope, scope)
                    }))
            };

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            return new HouseNumberValidator(httpContextAccessor.Object);
        }
    }
}
