namespace AddressRegistry.Api.Legacy.AddressMatch
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}
