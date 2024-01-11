namespace AddressRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class EventsRepository : IEventsRepository

    {
    private readonly string _eventsConnectionString;

    public EventsRepository(string eventsConnectionString)
    {
        _eventsConnectionString = eventsConnectionString;
    }

    public async Task<int?> GetAddressPersistentLocalId(Guid addressId)
    {
        await using var connection = new SqlConnection(_eventsConnectionString);
        var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where s.Id = '{addressId}'";

        return await connection.QuerySingleOrDefaultAsync<int?>(sql);
    }
    }
}
