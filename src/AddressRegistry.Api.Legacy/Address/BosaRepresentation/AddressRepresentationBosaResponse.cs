namespace AddressRegistry.Api.Legacy.Address.BosaRepresentation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract]
    public class AddressRepresentationBosaResponse
    {
        /// <summary>
        /// the identifier of the address
        /// </summary>
        [Required]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// address representations
        /// </summary>
        [Required]
        public List<AddressRepresentationBosa> AdresVoorstellingen { get; set; }
    }

    [DataContract]
    public class AddressRepresentationBosa
    {
        [Required]
        public Taal Taal { get; set; }

        [Required]
        public string Huisnummer { get; set; }

        public string Busnummer { get; set; }

        [Required]
        public string VolledigAdres { get; set; }

        [Required]
        public string Gemeentenaam { get; set; }

        [Required]
        public string Straatnaam { get; set; }

        [Required]
        public string Postcode { get; set; }

        public AddressRepresentationBosa(Taal taal, string huisnummer, string busnummer, string volledigAdres, string gemeentenaam, string straatnaam, string postcode)
        {
            Taal = taal;
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            VolledigAdres = volledigAdres;
            Gemeentenaam = gemeentenaam;
            Straatnaam = straatnaam;
            Postcode = postcode;
        }
    }

    public class AddressRepresentationBosaResponseExamples : IExamplesProvider<AddressRepresentationBosaResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressRepresentationBosaResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressRepresentationBosaResponse GetExamples()
            => new AddressRepresentationBosaResponse
            {
                Identificator = new AdresIdentificator(_responseOptions.Naamruimte, "12345", DateTimeOffset.Now.ToExampleOffset()),
                AdresVoorstellingen = new List<AddressRepresentationBosa>
                {
                    new AddressRepresentationBosa(Taal.NL, "5", "1", "Teststraat 5 bus 1, 9000 Gent", "Gent", "Teststraat", "9000"),
                    new AddressRepresentationBosa(Taal.FR, "5", "1", "Rue de test 5 bte 1, 9000 Gand", "Gand", "Rue de test", "9000")
                }
            };
    }
}

