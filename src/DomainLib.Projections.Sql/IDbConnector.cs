using System.Collections.Generic;
using System.Data.Common;

namespace DomainLib.Projections.Sql
{
    public interface IDbConnector
    {
        SqlContextSettings ContextSettings => SqlContextSettings.Default;
        DbConnection CreateConnection();
        DbCommand BuildUpsertCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap);
        DbCommand BuildDeleteCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap);
        void BindParameters<TEvent>(DbCommand command, TEvent @event, EventSqlColumnDefinitions eventPropertyMap);
        string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions);
    }
}