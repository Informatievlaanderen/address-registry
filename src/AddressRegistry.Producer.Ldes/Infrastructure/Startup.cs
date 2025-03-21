namespace AddressRegistry.Producer.Ldes.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    public sealed class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors();

            app.UseHealthChecks("/health");

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var baseUri = configuration.GetValue<string>("BaseUrl").TrimEnd('/');
            app.UseProjectorEndpoints(baseUri, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}
