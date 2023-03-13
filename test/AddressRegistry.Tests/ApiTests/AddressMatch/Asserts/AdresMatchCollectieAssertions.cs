namespace AddressRegistry.Tests.ApiTests.AddressMatch.Asserts
{
    using System.Collections.Generic;
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using FluentAssertions;

    internal class AdresMatchCollectieAssertions : Assertions<AddressMatchCollection, AdresMatchCollectieAssertions>
    {
        public AdresMatchCollectieAssertions(AddressMatchCollection subject)
            : base(subject) { }

        public AndWhichConstraint<AdresMatchCollectieAssertions, List<AdresMatchItem>> HaveMatches(int matchCount)
        {
            AssertingThat($"[{matchCount}] match(es) were found");

            Subject.AdresMatches.Should().HaveCount(matchCount);

            return AndWhich(Subject.AdresMatches);
        }

        public AndConstraint<AdresMatchCollectieAssertions> ContainWarning(string warningMessagePart)
        {
            AssertingThat($"a warning containing [{warningMessagePart}] was present");

            Subject.Warnings.Should().Contain(w => w.Message.Contains(warningMessagePart));

            return And();
        }

        public AndConstraint<AdresMatchCollectieAssertions> NotContainWarning(string warningMessagePart)
        {
            AssertingThat($"a warning containing [{warningMessagePart}] was present");

            Subject.Warnings.Should().NotContain(w => w.Message.Contains(warningMessagePart));

            return And();
        }
    }
}
