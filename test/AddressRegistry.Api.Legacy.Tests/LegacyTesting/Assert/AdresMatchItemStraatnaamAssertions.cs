namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using AddressMatch.Responses;
    using FluentAssertions;

    public class AdresMatchItemStraatnaamAssertions : Assertions<AdresMatchItemStraatnaam, AdresMatchItemStraatnaamAssertions>
    {
        public AdresMatchItemStraatnaamAssertions(AdresMatchItemStraatnaam subject) : base(subject)
        {
        }

        internal AndConstraint<AdresMatchItemStraatnaamAssertions> HaveStraatnaam(string straatnaam)
        {
            AssertingThat($"'Straatnaam' is [{straatnaam}]");

            Subject.Straatnaam.GeografischeNaam.Spelling.Should().Be(straatnaam);

            return And();
        }

        internal AndConstraint<AdresMatchItemStraatnaamAssertions> HaveObjectId(string objectId)
        {
            AssertingThat($"'ObjectId' is [{objectId}]");

            Subject.ObjectId.Should().Be(objectId);

            return And();
        }
    }
}
