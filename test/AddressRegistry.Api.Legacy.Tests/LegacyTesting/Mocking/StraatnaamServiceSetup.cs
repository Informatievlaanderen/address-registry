namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressMatch.Matching;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.StreetName;

    public class KadRrServiceSetup : MockingSetup<IKadRrService>
    {
        internal KadRrServiceSetup StraatnamenExistForKadStraatcodeAndNISCode(IEnumerable<StreetNameLatestItem> straatnamen, string kadStraatCode, string nisCode)
        {
            When($"[{straatnamen.Count()}] straatnamen exist for kadStraatcode [{kadStraatCode}] and nisCode[{nisCode}]\r\n{straatnamen.ToLoggableString(LogFormatting)}");

            Moq.Setup(m => m.GetStreetNamesByKadStreet(kadStraatCode, nisCode)).Returns(straatnamen);

            return this;
        }

        internal KadRrServiceSetup StraatnaamExistsForRrStraatcodeAndPostcode(StreetNameLatestItem straatnaam, string rrStraatcode, string postcode)
        {
            When($"straatnaam exists for rrStraatcode [{rrStraatcode}] and postcode[{postcode}]\r\n[{straatnaam.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetStreetNameByRrStreet(rrStraatcode, postcode)).Returns(straatnaam);

            return this;
        }

        internal KadRrServiceSetup AdresMappingExistsFor(IEnumerable<AddressDetailItem> adressen, string huisnummer, string index, string rrStraatcode, string postcode)
        {
            When($"rr adres mapping exists for huisnummer [{huisnummer}], index [{index}], rrStraatcode [{rrStraatcode}] and postcode[{postcode}]\r\n[{adressen.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetAddressesBy(huisnummer, index, rrStraatcode, postcode)).Returns(adressen);

            return this;
        }
    }
}
