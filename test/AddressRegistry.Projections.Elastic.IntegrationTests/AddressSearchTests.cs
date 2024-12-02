namespace AddressRegistry.Projections.Elastic.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressSearch;
    using Api.Oslo.Infrastructure.Elastic;
    using FluentAssertions;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Transport;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public sealed class AddressSearchTests
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly AddressApiElasticsearchClient _addressListClient;
        private readonly string _elasticAlias;

        public AddressSearchTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var elasticConfig = configuration.GetSection("Elastic");
            var elasticUrl = elasticConfig.GetValue<string>("Uri") ?? throw new ArgumentNullException("Elastic Uri is not set");
            var elasticUsername = elasticConfig.GetValue<string>("Username");
            var elasticPassword = elasticConfig.GetValue<string>("Password");
            _elasticAlias = elasticConfig.GetValue<string>("IndexAlias") ?? throw new ArgumentNullException("Elastic IndexAlias is not set");

            var clientSettings = new ElasticsearchClientSettings(new Uri(elasticUrl))
                .DefaultIndex(_elasticAlias)
                .EnableDebugMode()
                .DisableDirectStreaming();

            if (!string.IsNullOrWhiteSpace(elasticUsername) && !string.IsNullOrWhiteSpace(elasticPassword))
                clientSettings.Authentication(new BasicAuthentication(elasticUsername, elasticPassword));

            _elasticClient = new ElasticsearchClient(clientSettings);
            _addressListClient = new AddressApiElasticsearchClient(_elasticClient, _elasticAlias, new NullLoggerFactory());
        }

        [Fact(Skip = "This is a test that should be run manually")]
        public async Task Test()
        {
            var outputs = new List<OutputTestCase>();
            var inputFileAsString = await File.ReadAllTextAsync("SearchCases/input.json");
            var inputTestCase = _elasticClient.SourceSerializer.Deserialize<InputTestCase>(new MemoryStream(Encoding.UTF8.GetBytes(inputFileAsString)));
            var jsonString = inputTestCase.QueryObject.ToJsonString();

            foreach (var input in inputTestCase.Inputs)
            {
                var formattedJsonString = jsonString.Replace("{{input}}", input);

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(formattedJsonString));
                var searchRequest = _elasticClient.SourceSerializer.Deserialize<SearchRequestWithIndex>(stream);
                searchRequest.SetIndex(_elasticAlias);

                var addressResponses = await _addressListClient.SearchAddresses(input, null, null);

                var response = await _elasticClient.SearchAsync<AddressSearchDocument>(searchRequest);

                response.IsValidResponse.Should().BeTrue(response.DebugInformation);
                response.Documents.Should().NotBeEmpty();

                var language = addressResponses.Language;

                outputs.Add(new OutputTestCase(
                    input,
                    response.Documents.Select(x => x.FullAddress.Single(name => name.Language == language).Spelling).ToArray(),
                    addressResponses.Addresses.Select(x => x.FullAddress.Single(name => name.Language == language).Spelling).ToArray(),
                    language));
            }

            await File.WriteAllTextAsync("SearchCases/output.md",
                FormatOutput(inputTestCase.QueryObject.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true,
                }), outputs));
        }

        private static string FormatOutput(string jsonString, List<OutputTestCase> outputs)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("# Outputs");

            stringBuilder.AppendLine("## JSON Query");
            stringBuilder.AppendLine("```json");
            stringBuilder.AppendLine(jsonString);
            stringBuilder.AppendLine("```");

            stringBuilder.AppendLine("## Results");
            foreach (var output in outputs)
            {
                stringBuilder.AppendLine($"### Input: {output.Input}");
                stringBuilder.AppendLine($"Language: {output.Language}");

                if (output.AreResultsEqual)
                {
                    stringBuilder.AppendLine("#### Elastic/AddressClient");
                    stringBuilder.AppendLine("```");
                    foreach (var result in output.ElasticResults)
                    {
                        stringBuilder.AppendLine(result);
                    }
                    stringBuilder.AppendLine("```");
                }
                else
                {
                    stringBuilder.AppendLine("#### Elastic");
                    stringBuilder.AppendLine("```");
                    foreach (var result in output.ElasticResults)
                    {
                        stringBuilder.AppendLine(result);
                    }
                    stringBuilder.AppendLine("```");

                    stringBuilder.AppendLine("#### AddressClient");
                    stringBuilder.AppendLine("```");
                    foreach (var result in output.AddressClientResults)
                    {
                        stringBuilder.AppendLine(result);
                    }

                    stringBuilder.AppendLine("```");
                }
            }

            return stringBuilder.ToString();
        }

        private sealed class SearchRequestWithIndex : SearchRequest
        {
            public void SetIndex(string indexName)
            {
                RouteValues.Add("index", indexName);
            }
        }
    }

    public sealed class InputTestCase
    {
        public JsonObject QueryObject { get; set; }
        public string[] Inputs { get; set; }
    }

    public sealed class OutputTestCase
    {
        public string Input { get; set; }
        public string[] ElasticResults { get; set; }
        public string[] AddressClientResults { get; set; }
        public Language? Language { get; }

        public OutputTestCase(string input, string[] elasticResults, string[] addressClientResults, Language? language)
        {
            Input = input;
            ElasticResults = elasticResults;
            AddressClientResults = addressClientResults;
            Language = language;
        }

        public bool AreResultsEqual => ElasticResults.SequenceEqual(AddressClientResults);
    }
}
