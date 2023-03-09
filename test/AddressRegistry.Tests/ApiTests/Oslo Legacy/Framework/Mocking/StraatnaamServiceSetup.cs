namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Mocking
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Api.Oslo.AddressMatch.V1.Matching;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Syndication.StreetName;

    public class KadRrServiceSetup : MockingSetup<IKadRrService>
    {
        internal KadRrServiceSetup StraatnamenExistForKadStraatcodeAndNisCode(
            IEnumerable<StreetNameLatestItem> straatnamen,
            string kadStraatCode,
            string nisCode)
        {
            var streetNameLatestItems = straatnamen.ToList();

            When($"[{streetNameLatestItems.Count()}] straatnamen exist for kadStraatcode [{kadStraatCode}] and nisCode[{nisCode}]\r\n{streetNameLatestItems.ToLoggableString(LogFormatting)}");

            Moq.Setup(m => m.GetStreetNamesByKadStreet(kadStraatCode, nisCode)).Returns(streetNameLatestItems);

            return this;
        }

        internal KadRrServiceSetup StraatnaamExistsForRrStraatcodeAndPostcode(
            StreetNameLatestItem straatnaam,
            string rrStraatcode,
            string postcode)
        {
            When($"straatnaam exists for rrStraatcode [{rrStraatcode}] and postcode[{postcode}]\r\n[{straatnaam.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetStreetNameByRrStreet(rrStraatcode, postcode)).Returns(straatnaam);

            return this;
        }

        internal KadRrServiceSetup AdresMappingExistsFor(
            IEnumerable<AddressDetailItem> adressen,
            string huisnummer,
            string index,
            string rrStraatcode,
            string postcode)
        {
            var addressDetailItems = adressen.ToList();

            When($"rr adres mapping exists for huisnummer [{huisnummer}], index [{index}], rrStraatcode [{rrStraatcode}] and postcode[{postcode}]\r\n[{addressDetailItems.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetAddressesBy(huisnummer, index, rrStraatcode, postcode)).Returns(addressDetailItems);

            return this;
        }
    }
}
