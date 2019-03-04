namespace AddressRegistry.Tests.AutoFixture
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using global::AutoFixture;
    using global::AutoFixture.Dsl;
    using global::AutoFixture.Kernel;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public class SetProvenanceImplementationsCallSetProvenance:ICustomization
    {
        public void Customize(IFixture fixture)
        {
            bool IsEventNamespace(Type t) => t.Namespace.EndsWith("Events");
            bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

            var provenanceEventTypes = typeof(DomainAssemblyMarker).Assembly
                    .GetTypes()
                    .Where(t => t.IsClass && t.Namespace != null && IsEventNamespace(t) && IsNotCompilerGenerated(t) && t.GetInterfaces().Any(i => i == typeof(ISetProvenance)))
                    .ToList();

            foreach (var allEventType in provenanceEventTypes)
            {
                var getSetProvenanceMethod = GetType()
                    .GetMethod("GetSetProvenance", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(allEventType);
                var setProvenanceDelegate = getSetProvenanceMethod.Invoke(this, new object[] {fixture.Create<Provenance>()});

                var customizeMethod = typeof(Fixture).GetMethods().Single(m => m.Name == "Customize" && m.IsGenericMethod);
                var genericCustomizeMethod = customizeMethod.MakeGenericMethod(allEventType);
                genericCustomizeMethod.Invoke(fixture, new object[] { setProvenanceDelegate });
            }
        }

        private Func<ICustomizationComposer<T> ,ISpecimenBuilder> GetSetProvenance<T>(Provenance provenance)
            where T : ISetProvenance
        {
            return c=>c.Do(@event =>
                (@event as ISetProvenance).SetProvenance(provenance));
        }
    }
}
