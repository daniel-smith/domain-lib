using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace DomainLib.Projections.Sql
{
    public interface ISqlDialect
    {
        public DbCommand BuildUpsertCommand(ISqlProjection projection, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap);
        public DbCommand BuildDeleteCommand(ISqlProjection projection, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap);
        void BindParameters<TEvent>(DbCommand command, TEvent @event, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap);
        DbConnection CreateConnection();
        string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions);
    }
}