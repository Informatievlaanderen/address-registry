namespace AddressRegistry.Api.Oslo.AddressMatch
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}
