namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using NodaTime;

    public partial class NodaTimeCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new LocalDateGenerator());
            fixture.Customizations.Add(new LocalTimeGenerator());
            fixture.Customizations.Add(new InstantGenerator());
            fixture.Customizations.Add(new LocalDateTimeGenerator());
        }

        public class LocalTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalTime.FromTicksSinceMidnight(DateTime.Now.TimeOfDay.Ticks);
            }
        }

        public class InstantGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(Instant).Equals(request))
                {
                    return new NoSpecimen();
                }

                return Instant.FromDateTimeOffset(DateTimeOffset.Now);
            }
        }

        public class LocalDateTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalDateTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalDateTime.FromDateTime(DateTime.Now);
            }
        }
    }
}
