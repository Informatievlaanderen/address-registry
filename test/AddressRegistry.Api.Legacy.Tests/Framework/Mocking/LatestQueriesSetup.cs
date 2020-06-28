namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Legacy.AddressMatch.Matching;
    using Moq;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;

    public class LatestQueriesSetup : MockingSetup<ILatestQueries>
    {
        internal LatestQueriesSetup LatestGemeentesExist(IEnumerable<MunicipalityLatestItem> gemeentes)
        {
            var municipalityLatestItems = gemeentes.ToList();

            When($"[{municipalityLatestItems.Count()}] gemeentes exist\r\n[{municipalityLatestItems.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetAllLatestMunicipalities()).Returns(municipalityLatestItems.AsQueryable());

            return this;
        }

        internal LatestQueriesSetup LatestStraatnamenExistForGemeentenaam(IEnumerable<StreetNameLatestItem> straatnamen, string gemeentenaam)
        {
            var streetNameLatestItems = straatnamen.ToList();

            When($"[{streetNameLatestItems.Count()}] straatnamen exist for gemeentenaam [{gemeentenaam}]\r\n[{streetNameLatestItems.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetLatestStreetNamesBy(gemeentenaam)).Returns(streetNameLatestItems.AsQueryable());
            Moq.Setup(m => m.GetAllLatestStreetNames()).Returns(streetNameLatestItems.AsQueryable());

            foreach (StreetNameLatestItem straatnaam in streetNameLatestItems)
                Moq.Setup(m => m.FindLatestStreetNameById(straatnaam.PersistentLocalId)).Returns(straatnaam);

            return this;
        }

        internal LatestQueriesSetup LatestGemeentes(IEnumerable<MunicipalityLatestItem> gemeentes)
        {
            Moq.Setup(x => x.GetAllLatestMunicipalities()).Returns(gemeentes);
            return this;
        }

        internal LatestQueriesSetup LatestStraatNamen(IEnumerable<StreetNameLatestItem> streetNames)
        {
            Moq.Setup(x => x.GetAllLatestStreetNames()).Returns(streetNames);
            return this;
        }

        internal LatestQueriesSetup LatestPostInfo(IEnumerable<PostalInfoLatestItem> postalInfos)
        {
            Moq.Setup(x => x.GetAllPostalInfo()).Returns(postalInfos);
            return this;
        }

        internal LatestQueriesSetup LatestAdressenExist(IEnumerable<AddressDetailItem> adressen, string straatnaamId, string huisnummer, string busnummer)
        {
            var addressDetailItems = adressen.ToList();

            When($"[{addressDetailItems.Count()}] adressen exist for straatnaamId [{straatnaamId}], huisnummer [{huisnummer}] and busnummer [{busnummer}]\r\n{addressDetailItems.ToLoggableString(LogFormatting)}");

            Moq.Setup(m => m.GetLatestAddressesBy(straatnaamId, huisnummer, busnummer)).Returns(addressDetailItems.AsQueryable());

            return this;
        }

        internal LatestQueriesSetup InterceptingLatestAdresQuery(Action<string, string, string> callback, IEnumerable<AddressDetailItem> returnResult)
        {
            When($"intercepting the adres search query");

            Moq.Setup(m => m.GetLatestAddressesBy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(returnResult.AsQueryable()).Callback(callback);

            return this;
        }
    }
}
