namespace AddressRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using Duende.IdentityModel;
    using Duende.IdentityModel.Client;
    using Infrastructure;
    using Infrastructure.Modules;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class IntegrationTestFixture : IAsyncLifetime
    {
        private string _clientId;
        private string _clientSecret;
        private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();

        public TestServer TestServer { get; private set; }
        public SqlConnection SqlConnection { get; private set; }

        public async Task InitializeAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = configuration.GetValue<string>("ClientId");
            _clientSecret = configuration.GetValue<string>("ClientSecret");

            _ = DockerComposer.Compose("sqlserver.yml", "address-integration-tests");
            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();

            var hostBuilder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new NullLoggerFactory();
                    builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));
                })
                .ConfigureWebHost(webhostBuilder =>
                {
                    webhostBuilder.UseConfiguration(configuration)
                        .UseStartup<Startup>()
                        .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                        .UseTestServer();
                })
                .Build();

            await hostBuilder.StartAsync();

            TestServer = hostBuilder.GetTestServer();
        }

        private async Task WaitForSqlServerToBecomeAvailable()
        {
            foreach (var _ in Enumerable.Range(0, 60))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (await OpenConnection())
                {
                    break;
                }
            }
        }

        private async Task<bool> OpenConnection()
        {
            try
            {
                SqlConnection = new SqlConnection("Server=localhost,5436;User Id=sa;Password=Pass@word;database=master;TrustServerCertificate=True;");
                await SqlConnection.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task CreateDatabase()
        {
            var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'address-registry') BEGIN CREATE DATABASE [address-registry] END", SqlConnection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<string> GetAccessToken(string requiredScopes)
        {
            if (_accessTokens.ContainsKey(requiredScopes) && !_accessTokens[requiredScopes].IsExpired)
            {
                return _accessTokens[requiredScopes].Token;
            }

            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessTokens[requiredScopes] = new AccessToken(response.AccessToken, response.ExpiresIn);

            return _accessTokens[requiredScopes].Token;
        }

        public async Task DisposeAsync()
        {
            await SqlConnection.DisposeAsync();
        }
    }

    public class AccessToken
    {
        private readonly DateTime _expiresAt;

        public string Token { get; }

        // Let's regard it as expired 10 seconds before it actually expires.
        public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

        public AccessToken(string token, int expiresIn)
        {
            _expiresAt = DateTime.Now.AddSeconds(expiresIn);
            Token = token;
        }
    }
}
