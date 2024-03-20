namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System.Linq;
    using System.Reflection;
    using Validators;
    using Autofac;
    using FluentValidation;
    using Module = Autofac.Module;

    public class FluentValidationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(ProposeAddressRequestValidator).GetTypeInfo().Assembly)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
                .AsImplementedInterfaces();
        }
    }
}
