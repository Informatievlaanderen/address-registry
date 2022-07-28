namespace AddressRegistry.Projector.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        protected Program()
        { }
        
        public static void Main(string[] args)
            => Run(new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = 5006
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    },
                    MiddlewareHooks =
                    {
                        ConfigureDistributedLock =
                            configuration => DistributedLockOptions.LoadFromConfiguration(configuration)
                    }
                });

        private static void Run(ProgramOptions options)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(options)
                .RunWithLock<Program>();
    }
}
