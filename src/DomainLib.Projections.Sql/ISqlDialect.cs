using System.Collections.Generic;
using System.Data.Common;

namespace DomainLib.Projections.Sql
{
    public interface ISqlDialect
    {
        public DbCommand BuildUpsertCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap);
        public DbCommand BuildDeleteCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap);
        void BindParameters<TEvent>(DbCommand command, TEvent @event, EventSqlColumnDefinitions eventPropertyMap);
        DbConnection CreateConnection();
        string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions);
    }
}