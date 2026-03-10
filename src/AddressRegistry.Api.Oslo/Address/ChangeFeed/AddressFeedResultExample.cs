namespace AddressRegistry.Api.Oslo.Address.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class AddressFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _feedConfig;

        public AddressFeedResultExample(IOptions<ResponseOptions> feedConfig)
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
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/adres/3064179",
                                     "objectId": "3064179",
                                     "naamruimte": "https://data.vlaanderen.be/id/adres",
                                     "versieId": "2023-11-01T11:44:38+01:00",
                                     "nisCodes": [
                                         "44935"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "straatnaam.id",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": 44935
                                         },
                                         {
                                             "naam": "adresStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "inGebruik"
                                         },
                                         {
                                             "naam": "huisnummer",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "174B"
                                         },
                                         {
                                             "naam": "postcode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "8310"
                                         },
                                         {
                                             "naam": "officieelToegekend",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": true
                                         },
                                         {
                                             "naam": "positieGeometrieMethode",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "afgeleidVanObject"
                                         },
                                         {
                                             "naam": "positieSpecificatie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "gebouweenheid"
                                         },
                                         {
                                             "naam": "adresPositie",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "type": "Point",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/31370",
                                                     "gml": "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>73862.07 211634.58</gml:pos></gml:Point>"
                                                 },
                                                 {
                                                     "type": "Point",
                                                     "projectie": "http://www.opengis.net/def/crs/EPSG/0/3812",
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
