namespace AddressRegistry.Api.Legacy.Address.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class AddressRepresentationBosaResponse
    {
        /// <summary>
        /// the identifier of the street name
        /// </summary>
        [Required]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// address representations
        /// </summary>
        [Required]
        public List<BosaAddressRepresentation> AdresVoorstellingen { get; set; }
    }

    public class BosaAddressRepresentation
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

        public BosaAddressRepresentation(Taal taal, string huisnummer, string busnummer, string volledigAdres, string gemeentenaam, string straatnaam, string postcode)
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

    public class AddressRepresentationBosaResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public AddressRepresentationBosaResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            return new AddressRepresentationBosaResponse
            {
                Identificator = new Identificator(_responseOptions.Naamruimte, "12345", DateTimeOffset.Now),
                AdresVoorstellingen = new List<BosaAddressRepresentation>
                {
                    new BosaAddressRepresentation(Taal.NL, "5", "1", "Teststraat 5 bus 1, 9000 Gent", "Gent", "Teststraat", "9000"),
                    new BosaAddressRepresentation(Taal.FR, "5", "1", "Rue de test 5 bte 1, 9000 Gand", "Gand", "Rue de test", "9000")
                }
            };
        }
    }
}

