namespace AddressRegistry.Projector.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        private static readonly DevelopmentCertificate DevelopmentCertificate = new DevelopmentCertificate(
            "api.dev.adres.basisregisters.vlaanderen.be.pfx",
            "gemeenteregister!");

        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(
                    new ProgramOptions
                    {
                        Hosting =
                        {
                            HttpPort = 4093,
                            HttpsPort = 4447,
                            HttpsCertificate = DevelopmentCertificate.ToCertificate
                        },
                        Runtime =
                        {
                            CommandLineArgs = args
                        }
                    });
    }
}
