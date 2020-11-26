using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public class SqlContext : IContext
    {
        private readonly StringBuilder _schemaStringBuilder = new StringBuilder();
        private IDbTransaction _activeTransaction;

        public SqlContext(DbConnection connection)
        {
            Connection = connection;
        }

        public DbConnection Connection { get; }


        public async Task OnSubscribing()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();

                var createSchemaCommand = Connection.CreateCommand();
                createSchemaCommand.CommandText = _schemaStringBuilder.ToString();
                await createSchemaCommand.ExecuteNonQueryAsync();
            }
        }

        public Task OnCaughtUp()
        {
            return Task.CompletedTask;
        }

        public async Task OnBeforeHandleEvent()
        {
            _activeTransaction = await Connection.BeginTransactionAsync();
        }

        public Task OnAfterHandleEvent()
        {
            _activeTransaction.Commit();
            return Task.CompletedTask;
        }

        public void AddSchema(string createSchemaSql)
        {
            _schemaStringBuilder.Append($"{createSchemaSql} ");
        }
    }
}