namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Assert
{
    using System.Threading.Tasks;
    using AddressMatch;
    using FluentAssertions;

    internal class AdresListFiltersTaskAssertions : TaskContinuationAssertion<AdresListFilterStub, AdresListFiltersTaskAssertions>
    {
        public AdresListFiltersTaskAssertions(Task<AdresListFilterStub> subject)
            : base(subject) { }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveHuisnummer(string expectedHuisnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Huisnummer' is '{expectedHuisnummer}'");

                result.HuisnummerFilter.Should().Be(expectedHuisnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveBusnummer(string expectedBusnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Busnummer' is '{expectedBusnummer}'");

                result.BusnummerFilter.Should().Be(expectedBusnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveAppnummer(string expectedAppnummer)
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Appnummer' is '{expectedAppnummer}' NOT IMPLEMENTED, WHAT TODO WITH APPNUMMER?");

                //result.Appnummer.Should().Be(expectedAppnummer);
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveNoHuisnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Huisnummer' is <null>");

                result.HuisnummerFilter.Should().BeNull();
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveNoBusnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Busnummer' is <null>");

                result.BusnummerFilter.Should().BeNull();
            });

            return AndContinuation();
        }

        public AndContinuationConstraint<AdresListFilterStub, AdresListFiltersTaskAssertions> HaveNoAppnummer()
        {
            AddAssertion(result =>
            {
                AssertingThat($"the 'Appnummer' is <null> NOT IMPLEMENTED, WHAT TODO WITH APPNUMMER?");

                //result.Appnummer.Should().BeNull();
            });

            return AndContinuation();
        }
    }
}
