namespace AddressRegistry.Api.Oslo.Address.V3.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class AddressFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptionsV3 _feedConfig;

        public AddressFeedResultExample(IOptions<ResponseOptionsV3> feedConfig)
        {
            _feedConfig = feedConfig.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                            {
                                "specversion": "1.0",
                                 "id": "1",
                                 "time": "2023-11-01T11:44:38.5493268+01:00",
                                 "type": "basisregisters.address.create.v1",
                                 "source": "{{_feedConfig.AddressFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_feedConfig.AddressFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "AddressWasMigratedToStreetName",
                                 "basisregisterscausationid": "4fe743fb-0736-5246-8df2-da07f9276c88",
                                 "subject": "https://data.vlaanderen.be/id/adres/3064179",
                                 "data": {
                                     "objectId": "3064179",
                                     "naamruimte": "https://data.vlaanderen.be/id/adres",
                                     "versieId": "2023-11-01T11:44:38+01:00",
                                     "nisCodes": [
                                         "44935"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "heeftStraatnaam",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/straatnaam/44935"
                                         },
                                         {
                                             "naam": "status",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/adresstatus/inGebruik"
                                         },
                                         {
                                             "naam": "huisnummer",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "174B"
                                         },
                                         {
                                             "naam": "heeftPostinfo",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/postinfo/8310"
                                         },
                                         {
                                             "naam": "officieelToegekend",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": true
                                         },
                                         {
                                             "naam": "positie.methode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/geometriemethode/afgeleidVanObject"
                                         },
                                         {
                                             "naam": "positie.specificatie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/geometriespecificatie/gebouweenheid"
                                         },
                                         {
                                             "naam": "positie.geometrie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "type": "Punt",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>73862.07 211634.58</gml:pos></gml:Point>"
                                                 },
                                                 {
                                                     "type": "Punt",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>573857.26 711625.49</gml:pos></gml:Point>"
                                                 }
                                             ]
                                         }
                                     ]
                                 }
                             }
                         ]
                         """;
            return JArray.Parse(json);
        }
    }
}
