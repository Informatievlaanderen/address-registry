namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using AddressMatch.Matching;
    using Moq;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LatestQueriesSetup : MockingSetup<ILatestQueries>
    {
        internal LatestQueriesSetup LatestGemeentesExist(IEnumerable<MunicipalityLatestItem> gemeentes)
        {
            When($"[{gemeentes.Count()}] gemeentes exist\r\n[{gemeentes.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetAllLatestMunicipalities()).Returns(gemeentes.AsQueryable());

            return this;
        }

        internal LatestQueriesSetup LatestStraatnamenExistForGemeentenaam(IEnumerable<StreetNameLatestItem> straatnamen, string gemeentenaam)
        {
            When($"[{straatnamen.Count()}] straatnamen exist for gemeentenaam [{gemeentenaam}]\r\n[{straatnamen.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetLatestStreetNamesBy(new[] { gemeentenaam })).Returns(straatnamen.AsQueryable());
            Moq.Setup(m => m.GetAllLatestStreetNames()).Returns(straatnamen.AsQueryable());
            foreach (StreetNameLatestItem straatnaam in straatnamen)
            {
                Moq.Setup(m => m.FindLatestStreetNameById(straatnaam.PersistentLocalId)).Returns(straatnaam);
            }

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
            When($"[{adressen.Count()}] adressen exist for straatnaamId [{straatnaamId}], huisnummer [{huisnummer}] and busnummer [{busnummer}]\r\n{adressen.ToLoggableString(LogFormatting)}");

            Moq.Setup(m => m.GetLatestAddressesBy(straatnaamId, huisnummer, busnummer)).Returns(adressen.AsQueryable());

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
