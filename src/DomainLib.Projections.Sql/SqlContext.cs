using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public class SqlContext : IContext
    {
        private readonly ISqlDialect _dialect;
        private readonly HashSet<ISqlProjection> _projections = new HashSet<ISqlProjection>();

        private readonly StringBuilder _schemaStringBuilder = new StringBuilder();
        private IDbTransaction _activeTransaction;

        public SqlContext(ISqlDialect dialect)
        {
            _dialect = dialect;
            Connection = dialect.CreateConnection();
        }

        public DbConnection Connection { get; }


        public async Task OnSubscribing()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
                await CreateSchema();
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
            _activeTransaction = null;
            return Task.CompletedTask;
        }

        public void RegisterProjection(ISqlProjection projection)
        {
            if (_projections.Add(projection))
            {
                var createTableSql = string.IsNullOrEmpty(projection.CustomCreateTableSql)
                                         ? _dialect.BuildCreateTableSql(projection.TableName, projection.Columns.Values)
                                         : projection.CustomCreateTableSql;

                createTableSql = string.Concat(createTableSql, " ", projection.AfterCreateTableSql, " ");

                _schemaStringBuilder.Append(createTableSql);
            }
        }

        private async Task CreateSchema()
        {
            var createSchemaCommand = Connection.CreateCommand();
            
            createSchemaCommand.CommandText = _schemaStringBuilder.ToString();
            await createSchemaCommand.ExecuteNonQueryAsync();
        }
    }
}