namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using Projections.Syndication.PostalInfo;
    using System.Collections.Generic;
    using System.Linq;

    public class VbrPostinfoServiceSetup : MockingSetup<SyndicationContextMemory>
    {
        internal VbrPostinfoServiceSetup PostinfoExists(PostalInfoLatestItem existingPostinfo)
        {
            When($"Postinfo exists for Postcode [{existingPostinfo.PostalCode}]\r\n[{existingPostinfo.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.PostalInfoLatestItems.FirstOrDefault(x => x.PostalCode == existingPostinfo.PostalCode)).Returns(existingPostinfo);
            Moq.Setup(m => m.PostalInfoLatestItems.ToList()).Returns(new List<PostalInfoLatestItem> { existingPostinfo });

            return this;
        }
    }
}
