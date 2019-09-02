namespace AddressRegistry.Api.Legacy.CrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using CrabHouseNumber;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CrabSubadresCollectie", Namespace = "")]
    public class CrabSubAddressListResponse
    {
        /// <summary>
        /// De verzameling van adressen.
        /// </summary>
        [DataMember(Name = "CrabSubadressen", Order = 1)]
        public List<CrabSubAddressListItem> Addresses { get; set; }

        /// <summary>
        /// Het totaal aantal gemeenten die overeenkomen met de vraag.
        /// </summary>
        [DataMember(Name = "TotaalAantal", Order = 2)]
        public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 3, EmitDefaultValue = false)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "CrabSubadresCollectieItem", Namespace = "")]
    public class CrabSubAddressListItem
    {
        public CrabSubAddressListItem(
            int crabSubadresId,
            int persistentLocalId,
            string naamruimte,
            string detailUrl,
            string houseNumber,
            string boxNumber,
            VolledigAdres volledigAdres,
            DateTimeOffset version,
            bool isComplete)
        {
            CrabSubadresId = crabSubadresId;
            Address = new CrabAddressListItemAddress
            {
                Identificator = new Identificator(naamruimte, persistentLocalId.ToString(CultureInfo.InvariantCulture), version),
                IsComplete = isComplete,
                Detail = new Uri(string.Format(detailUrl, persistentLocalId)),
                Huisnummer = houseNumber,
                Busnummer = boxNumber,
                VolledigAdres = volledigAdres,
            };
        }

        /// <summary>
        /// Het CRAB HuisnummerId.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        public int CrabSubadresId { get; set; }

        /// <summary>
        ///	Het corresponderend address.
        /// </summary>
        [DataMember(Name = "Adres", Order = 2)]
        public CrabAddressListItemAddress Address { get; set; }
    }

    public class CrabSubaddressListResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public CrabSubaddressListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            var addressExamples = new List<CrabSubAddressListItem>
            {
                new CrabSubAddressListItem(
                    5,
                    10521,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "70",
                    "1",
                    new VolledigAdres("Koningin Maria Hendrikaplein", "70", null, "9000", "Gent", Taal.NL),
                    DateTimeOffset.Now,
                    true),
                new CrabSubAddressListItem(
                    157,
                    14874,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    "30",
                    "3",
                    new VolledigAdres("Boudewijnlaan", "30", "30", "1000", "Brussel", Taal.NL),
                    DateTimeOffset.Now.AddDays(-2),
                    false)
            };

            return new CrabSubAddressListResponse
            {
                Addresses = addressExamples,
                TotaalAantal = 2,
                Volgende = new Uri(string.Format(_responseOptions.CrabSubadressenVolgendeUrl, 2, 10))
            };
        }
    }
}
