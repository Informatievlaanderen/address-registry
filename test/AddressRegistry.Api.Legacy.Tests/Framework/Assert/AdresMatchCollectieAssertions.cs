namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Legacy.AddressMatch.Responses;
    using Newtonsoft.Json;

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

        public void BeEquivalentTo(AddressMatchCollection addressMatchCollection)
        {
            JsonConvert.SerializeObject(Subject).Should().Be(JsonConvert.SerializeObject(addressMatchCollection));
        }
    }
}
