//namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using Moq;

//    public class LatestVersionQueriesSetup : MockingSetup<ILatestVersionQueries>
//    {
//        internal LatestVersionQueriesSetup StraatnaamLatestVersionWithObjectIDsExistForGemeente(IEnumerable<StraatnaamLatestVersionWithObjectID> straatnamen, string gemeentenaam)
//        {
//            When($"[{straatnamen.Count()}] straatnamen exist for gemeente [{gemeentenaam}]\r\n{straatnamen.ToLoggableString(LogFormatting)}]");

//            global::Moq.Setup(m => m.GetStraatnaamLatestVersion(It.Is<IStraatnaamListFilters>(f => f!=null && f.GemeentenaamFilter == gemeentenaam))).Returns(straatnamen.AsQueryable());
//            global::Moq.Setup(m => m.GetStraatnaamLatestVersion(null)).Returns(straatnamen.AsQueryable());
//            foreach (StraatnaamLatestVersionWithObjectID straatnaam in straatnamen)
//                global::Moq.Setup(m => m.GetStraatnaamLatestVersion(straatnaam.StraatnaamID)).Returns(new[] { straatnaam }.AsQueryable());

//            return this;
//        }

//        internal LatestVersionQueriesSetup AdresLatestVersionWithObjectIDsExistFor(IEnumerable<AdresLatestVersionWithObjectID> adressen, string gemeentenaam, string straatnaam, string huisnummer, string busnummer)
//        {
//            When($"[{adressen.Count()}] adressen exist for gemeentenaam [{gemeentenaam}], straatnaam [{straatnaam}], huisnummer [{huisnummer}] and busnummer [{busnummer}]\r\n{adressen.ToLoggableString(LogFormatting)}]");

//            global::Moq.Setup(m => m.GetAdresLatestVersion(It.Is<IAdresListFilters>(f => f.GemeentenaamFilter == gemeentenaam && f.StraatnaamFilter == straatnaam && f.HuisnummerFilter == huisnummer && f.BusnummerFilter == busnummer)))
//                .Returns(adressen.AsQueryable());

//            foreach (AdresLatestVersionWithObjectID adres in adressen)
//                global::Moq.Setup(m => m.GetAdresLatestVersion(adres.AdresID)).Returns(new[] { adres }.AsQueryable());

//            return this;
//        }

//        internal LatestVersionQueriesSetup AdresLatestVersionWithObjectIDExist(AdresLatestVersionWithObjectID adres, int adresID)
//        {
//            When($"adres [{adresID}] exists\r\n[{adres.ToLoggableString(LogFormatting)}]");

//            global::Moq.Setup(m => m.GetAdresLatestVersion(adresID))
//                .Returns(new[] { adres }.AsQueryable());

//            return this;
//        }

//        internal LatestVersionQueriesSetup InterceptingAdresLatestVersionQuery(Action<IAdresListFilters> callback, IEnumerable<AdresLatestVersionWithObjectID> returnResult)
//        {
//            When($"intercepting the adres search filter");

//            global::Moq.Setup(m => m.GetAdresLatestVersion(It.IsAny<IAdresListFilters>())).Returns(returnResult.AsQueryable()).Callback(callback);

//            return this;
//        }

//        internal LatestVersionQueriesSetup GemeenteLatestVersionsExistsForNisCode(IEnumerable<GemeenteLatestVersionWithObjectID> gemeentes, string nisCode)
//        {
//            When($"[{gemeentes.Count()}] gemeentes exist for nisCode [{nisCode}]\r\n{gemeentes.ToLoggableString(LogFormatting)}]");

//            global::Moq.Setup(m => m.GetGemeenteLatestVersion(nisCode)).Returns(gemeentes.AsQueryable());
//            global::Moq.Setup(m => m.GetGemeenteLatestVersion()).Returns(gemeentes.AsQueryable());

//            return this;
//        }
//    }
//}
