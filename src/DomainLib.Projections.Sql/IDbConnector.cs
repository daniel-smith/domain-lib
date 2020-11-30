using System.Collections.Generic;
using System.Data.Common;

namespace DomainLib.Projections.Sql
{
    public interface IDbConnector
    {
        SqlContextSettings ContextSettings => SqlContextSettings.Default;
        DbConnection CreateConnection();
        string BuildUpsertCommandText(ISqlProjection projection, SqlColumnDefinitions columnDefinitions);
        string BuildDeleteCommandText(ISqlProjection projection, SqlColumnDefinitions columnDefinitions);
        string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions);
        void BindParameters<TEvent>(DbCommand command,
                                    TEvent @event,
                                    SqlColumnDefinitions columnDefinitions,
                                    ISqlParameterBindingMap<TEvent> parameterBindingMap);
    }
}