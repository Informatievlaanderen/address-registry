namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Microsoft.AspNetCore.Hosting;

    public sealed class Program
    {
        private Program()
        { }

        public static void Main(string[] args)
            => Run(new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = 5016
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
                        ConfigureDistributedLock = DistributedLockOptions.LoadFromConfiguration
                    }
                });

        private static void Run(ProgramOptions options)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(options)
                .RunWithLock<Program>();
    }
}
