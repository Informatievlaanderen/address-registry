namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System.Security.Claims;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Moq;

    public static class FakeHouseNumberValidator
    {
        private static HouseNumberValidator? _instance;
        public static HouseNumberValidator Instance => _instance ??= Create();

        private static HouseNumberValidator Create()
        {
            DefaultHttpContext httpContext = new()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(AcmIdmClaimTypes.Scope, Scopes.DvArAdresUitzonderingen)
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
