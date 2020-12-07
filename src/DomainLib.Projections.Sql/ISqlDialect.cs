using System.Collections.Generic;

namespace DomainLib.Projections.Sql
{
    public interface ISqlDialect
    {
        string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions);
        string BuildUpsertCommandText(ISqlProjection projection, SqlColumnDefinitions eventPropertyMap);
        string BuildDeleteCommandText(ISqlProjection projection, SqlColumnDefinitions eventPropertyMap);
    }
}