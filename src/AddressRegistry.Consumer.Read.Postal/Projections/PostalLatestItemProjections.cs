namespace AddressRegistry.Consumer.Read.Postal.Projections
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.PostalRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using NodaTime.Text;

    public sealed class PostalLatestItemProjections : ConnectedProjection<PostalConsumerContext>
    {
        public PostalLatestItemProjections()
        {
            When<PostalInformationWasRegistered>(async (context, message, ct) =>
            {
                var timestamp = InstantPattern.General.Parse(message.Provenance.Timestamp).Value;

                var postalLatestItem = new PostalLatestItem(message.PostalCode, timestamp);

                await context.PostalLatestItems.AddAsync(postalLatestItem, ct);
            });

            When<PostalInformationWasRealized>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postalLatestItem =>
                {
                    postalLatestItem.Status = PostalStatus.Current;
                    UpdateVersionTimestamp(message.Provenance, postalLatestItem);
                }, ct);
            });

            When<PostalInformationWasRetired>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postalLatestItem =>
                {
                    postalLatestItem.Status = PostalStatus.Retired;
                    UpdateVersionTimestamp(message.Provenance, postalLatestItem);
                }, ct);
            });

            When<PostalInformationPostalNameWasAdded>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postal =>
                {
                    postal.PostalNames.Add(
                        new PostalInfoPostalName(
                            message.PostalCode,
                            StringToPostalLanguage(message.Language),
                            message.Name));
                    UpdateVersionTimestamp(message.Provenance, postal);
                }, ct);
            });

            When<PostalInformationPostalNameWasRemoved>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postal =>
                {
                    var postalInformation = postal.PostalNames
                        .Single(p => p.Language == StringToPostalLanguage(message.Language) &&
                                     p.PostalName == message.Name);

                    postal.PostalNames.Remove(postalInformation);

                    UpdateVersionTimestamp(message.Provenance, postal);
                }, ct);
            });

            When<MunicipalityWasAttached>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postal =>
                {
                    postal.NisCode = message.NisCode;
                    UpdateVersionTimestamp(message.Provenance, postal);
                }, ct);
            });

            When<MunicipalityWasRelinked>(async (contextFactory, message, ct) =>
            {
                await contextFactory.FindAndUpdate(message.PostalCode, postal =>
                {
                    postal.NisCode = message.NewNisCode;
                    UpdateVersionTimestamp(message.Provenance, postal);
                }, ct);
            });
        }

        private static void UpdateVersionTimestamp(Provenance provenance, PostalLatestItem postalInformation)
        {
            var timestamp = InstantPattern.General.Parse(provenance.Timestamp).Value;
            postalInformation.VersionTimestamp = timestamp;
        }

        private static PostalLanguage StringToPostalLanguage(string language)
            => language.ToLower() switch
            {
                "dutch" => PostalLanguage.Dutch,
                "german" => PostalLanguage.German,
                "french" => PostalLanguage.French,
                "english" => PostalLanguage.English,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
    }
}
