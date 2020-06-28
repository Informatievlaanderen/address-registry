namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Legacy.AddressMatch.Matching;

    internal class HuisnummerWithSubadresTaskAssertions : TaskContinuationAssertion<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions>
    {
        public HuisnummerWithSubadresTaskAssertions(Task<HouseNumberWithSubaddress> subject)
            : base(subject) { }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveHuisnummer(string expectedHuisnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Huisnummer' is '{expectedHuisnummer}'");

                result.HouseNumber.Should().Be(expectedHuisnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveBusnummer(string expectedBusnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Busnummer' is '{expectedBusnummer}'");

                result.BoxNumber.Should().Be(expectedBusnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveAppnummer(string expectedAppnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Appnummer' is '{expectedAppnummer}'");

                result.AppNumber.Should().Be(expectedAppnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveNoHuisnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Huisnummer' is <null>");

                result.HouseNumber.Should().BeNull();
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveNoBusnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Busnummer' is <null>");

                result.BoxNumber.Should().BeNull();
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<HouseNumberWithSubaddress, HuisnummerWithSubadresTaskAssertions> HaveNoAppnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Appnummer' is <null>");

                result.AppNumber.Should().BeNull();
            });

            return AndContinuation();
        }
    }
}
