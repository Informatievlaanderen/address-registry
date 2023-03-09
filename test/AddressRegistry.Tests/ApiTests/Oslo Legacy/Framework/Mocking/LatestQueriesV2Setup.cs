namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using Moq;

    public class LatestQueriesV2Setup : MockingSetup<AddressRegistry.Api.Oslo.AddressMatch.V2.Matching.ILatestQueries>
    {
        internal LatestQueriesV2Setup LatestGemeentesExist(IEnumerable<MunicipalityLatestItem> gemeentes)
        {
            var municipalityLatestItems = gemeentes.ToList();

            When($"[{municipalityLatestItems.Count}] gemeentes exist\r\n[{municipalityLatestItems.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetAllLatestMunicipalities()).Returns(municipalityLatestItems.AsQueryable());

            return this;
        }

        internal LatestQueriesV2Setup LatestStraatnamenExistForGemeentenaam(IEnumerable<StreetNameLatestItem> straatnamen, string gemeentenaam)
        {
            var streetNameLatestItems = straatnamen.ToList();

            When($"[{streetNameLatestItems.Count()}] straatnamen exist for gemeentenaam [{gemeentenaam}]\r\n[{streetNameLatestItems.ToLoggableString(LogFormatting)}]");

            Moq.Setup(m => m.GetLatestStreetNamesBy(gemeentenaam)).Returns(streetNameLatestItems.AsQueryable());
            Moq.Setup(m => m.GetAllLatestStreetNames()).Returns(streetNameLatestItems.AsQueryable());

            foreach (StreetNameLatestItem straatnaam in streetNameLatestItems)
                Moq.Setup(m => m.FindLatestStreetNameById(straatnaam.PersistentLocalId)).Returns(straatnaam);

            return this;
        }

        internal LatestQueriesV2Setup LatestGemeentes(IEnumerable<MunicipalityLatestItem> gemeentes)
        {
            Moq.Setup(x => x.GetAllLatestMunicipalities()).Returns(gemeentes);
            return this;
        }

        internal LatestQueriesV2Setup LatestStraatNamen(IEnumerable<StreetNameLatestItem> streetNames)
        {
            Moq.Setup(x => x.GetAllLatestStreetNames()).Returns(streetNames);
            return this;
        }

        internal LatestQueriesV2Setup LatestPostInfo(IEnumerable<PostalInfoLatestItem> postalInfos)
        {
            Moq.Setup(x => x.GetAllPostalInfo()).Returns(postalInfos);
            return this;
        }

        internal LatestQueriesV2Setup LatestAdressenExist(IEnumerable<AddressDetailItemV2> adressen, int straatnaamId, string huisnummer, string busnummer)
        {
            var addressDetailItems = adressen.ToList();

            When($"[{addressDetailItems.Count}] adressen exist for straatnaamId [{straatnaamId}], huisnummer [{huisnummer}] and busnummer [{busnummer}]\r\n{addressDetailItems.ToLoggableString(LogFormatting)}");

            Moq.Setup(m => m.GetLatestAddressesBy(straatnaamId, huisnummer, busnummer)).Returns(addressDetailItems.AsQueryable());

            return this;
        }

        internal LatestQueriesV2Setup InterceptingLatestAdresQuery(Action<int, string, string> callback, IEnumerable<AddressDetailItemV2> returnResult)
        {
            When("intercepting the adres search query");

            Moq.Setup(m => m.GetLatestAddressesBy(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(returnResult.AsQueryable()).Callback(callback);

            return this;
        }
    }
}
