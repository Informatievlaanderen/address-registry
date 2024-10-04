namespace AddressRegistry.Infrastructure.Elastic
{
    public sealed class Name
    {
        public string Spelling { get; set; }
        public Language Language { get; set; }

        public Name()
        { }

        public Name(string spelling, Language language)
        {
            Spelling = spelling;
            Language = language;
        }
    }
}
