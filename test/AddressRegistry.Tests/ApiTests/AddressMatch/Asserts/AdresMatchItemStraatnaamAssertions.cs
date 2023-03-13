namespace AddressRegistry.Tests.ApiTests.AddressMatch.Asserts
{
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using FluentAssertions;

    internal class AdresMatchItemStraatnaamAssertions : Assertions<AdresMatchItemStraatnaam, AdresMatchItemStraatnaamAssertions>
    {
        public AdresMatchItemStraatnaamAssertions(AdresMatchItemStraatnaam subject)
            : base(subject) { }

        public AndConstraint<AdresMatchItemStraatnaamAssertions> HaveStraatnaam(string straatnaam)
        {
            AssertingThat($"'Straatnaam' is [{straatnaam}]");

            Subject.Straatnaam.GeografischeNaam.Spelling.Should().Be(straatnaam);

            return And();
        }

        public AndConstraint<AdresMatchItemStraatnaamAssertions> HaveObjectId(string objectId)
        {
            AssertingThat($"'ObjectId' is [{objectId}]");

            Subject.ObjectId.Should().Be(objectId);

            return And();
        }
    }
}
