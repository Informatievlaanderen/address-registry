namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using global::Elastic.Clients.Elasticsearch;

    public abstract class AddressApiElasticsearchClientBase
    {
        protected const string Keyword = "keyword";
        protected static readonly string NameSpelling = $"{ToCamelCase(nameof(AddressRegistry.Infrastructure.Elastic.Name.Spelling))}";

        protected ElasticsearchClient ElasticsearchClient { get; }
        protected string IndexAlias { get; }

        protected AddressApiElasticsearchClientBase(ElasticsearchClient elasticsearchClient, string indexAlias)
        {
            ElasticsearchClient = elasticsearchClient;
            IndexAlias = indexAlias;
        }

        protected static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
                return str;

            var chars = str.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (i == 0 || i > 0 && char.IsUpper(chars[i]))
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }
                else
                {
                    break;
                }
            }

            return new string(chars);
        }
    }
}
