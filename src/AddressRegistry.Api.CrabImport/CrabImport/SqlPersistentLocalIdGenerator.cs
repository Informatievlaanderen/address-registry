namespace AddressRegistry.Api.CrabImport.CrabImport
{
    using Address;
    using AddressRegistry.Infrastructure;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using System.Data;

    public class SqlPersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        private readonly SequenceContext _context;

        public SqlPersistentLocalIdGenerator(SequenceContext context) => _context = context;

        public PersistentLocalId GenerateNextPersistentLocalId()
        {
            var sqlStatement = $"SELECT NEXT VALUE FOR {Schema.Sequence}.{SequenceContext.AddressPersistentLocalIdSequenceName}";

            int nextNumber;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.Connection.Open();
                command.CommandText = sqlStatement;
                command.CommandType = CommandType.Text;

                nextNumber = (int)command.ExecuteScalar();
                command.Connection.Close();
            }

            return new PersistentLocalId(nextNumber);
        }
    }
}
