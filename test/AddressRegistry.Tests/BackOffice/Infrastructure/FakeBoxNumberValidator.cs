namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System.Security.Claims;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Moq;

    public static class FakeBoxNumberValidator
    {
        private static BoxNumberValidator? _interneBijwerkerInstance;
        private static BoxNumberValidator? _decentraleBijwerkerInstance;
        public static BoxNumberValidator InstanceInterneBijwerker => _interneBijwerkerInstance ??= Create(Scopes.DvArAdresUitzonderingen);
        public static BoxNumberValidator InstanceDecentraleBijwerker => _decentraleBijwerkerInstance ??= Create(Scopes.DvArAdresBeheer);

        private static BoxNumberValidator Create(string scope)
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

            return new BoxNumberValidator(actionContextAccessor.Object);
        }
    }
}
