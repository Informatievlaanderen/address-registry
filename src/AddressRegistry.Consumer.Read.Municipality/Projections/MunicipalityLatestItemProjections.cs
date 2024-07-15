namespace AddressRegistry.Consumer.Read.Municipality.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NodaTime.Text;

    public class MunicipalityLatestItemProjections : ConnectedProjection<MunicipalityConsumerContext>
    {
        private void UpdateVersionTimestamp(Provenance provenance, MunicipalityLatestItem municipality)
        {
            var timestamp = InstantPattern.General.Parse(provenance.Timestamp).Value;
            municipality.VersionTimestamp = timestamp;
        }

        public MunicipalityLatestItemProjections()
        {
            When<MunicipalityWasRegistered>(async (context, message, ct) =>
            {
                var timestamp = InstantPattern.General.Parse(message.Provenance.Timestamp).Value;

                var municipality = new MunicipalityLatestItem(
                    new Guid(message.MunicipalityId),
                    message.NisCode,
                    timestamp);

                await context.MunicipalityLatestItems.AddAsync(municipality, ct);
            });

            When<MunicipalityBecameCurrent>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.Status = MunicipalityStatus.Current;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityWasCorrectedToCurrent>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.Status = MunicipalityStatus.Current;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityWasCorrectedToRetired>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.Status = MunicipalityStatus.Retired;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityWasRetired>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.Status = MunicipalityStatus.Retired;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityWasDrawn>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.ExtendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityGeometryWasCorrectedToCleared>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.ExtendedWkbGeometry = null;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityGeometryWasCleared>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.ExtendedWkbGeometry = null;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityGeometryWasCorrected>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.ExtendedWkbGeometry = message.ExtendedWkbGeometry.ToByteArray();
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityWasNamed>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var taal = StringToTaal(message.Language);
                    SetMunicipalityName(taal, municipality, message.Name);
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityNameWasCleared>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var taal = StringToTaal(message.Language);
                    SetMunicipalityName(taal, municipality, null);
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityNameWasCorrected>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var taal = StringToTaal(message.Language);
                    SetMunicipalityName(taal, municipality, message.Name);
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityNameWasCorrectedToCleared>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var taal = StringToTaal(message.Language);
                    SetMunicipalityName(taal, municipality, null);
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityNisCodeWasDefined>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.NisCode = message.NisCode;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityNisCodeWasCorrected>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    municipality.NisCode = message.NisCode;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityOfficialLanguageWasAdded>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var newList = new List<string>();
                    newList.AddRange(municipality.OfficialLanguages);
                    newList.Add(message.Language);
                    municipality.OfficialLanguages = newList;
                    UpdateVersionTimestamp(message.Provenance, municipality);
                }, ct);
            });

            When<MunicipalityOfficialLanguageWasRemoved>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(new Guid(message.MunicipalityId), municipality =>
                {
                    var language = municipality.OfficialLanguages.FirstOrDefault(x => x == message.Language);

                    if (language != null)
                    {
                        var newList = new List<string>();
                        newList.AddRange(municipality.OfficialLanguages);
                        newList.Remove(message.Language);
                        municipality.OfficialLanguages = newList;
                        UpdateVersionTimestamp(message.Provenance, municipality);
                    }
                }, ct);
            });
        }

        private static Taal StringToTaal(string taal)
            => taal.ToLower() switch
            {
                "dutch" => Taal.NL,
                "german" => Taal.DE,
                "french" => Taal.FR,
                "english" => Taal.EN,
                _ => throw new ArgumentOutOfRangeException(nameof(taal), taal, null)
            };

        private static void SetMunicipalityName(Taal taal, MunicipalityLatestItem municipality, string? name)
        {
            switch (taal)
            {
                case Taal.NL:
                    municipality.NameDutch = name;
                    municipality.NameDutchSearch = name.RemoveDiacritics();
                    break;
                case Taal.DE:
                    municipality.NameGerman = name;
                    municipality.NameGermanSearch = name.RemoveDiacritics();
                    break;
                case Taal.FR:
                    municipality.NameFrench = name;
                    municipality.NameFrenchSearch = name.RemoveDiacritics();
                    break;
                case Taal.EN:
                    municipality.NameEnglish = name;
                    municipality.NameEnglishSearch = name.RemoveDiacritics();
                    break;
            }
        }
    }
}
