using DomainLib.Projections.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace DomainLib.Projections.Sqlite
{
    public class SqliteSqlDialect : ISqlDialect
    {
        private readonly string _connectionString;

        public SqliteSqlDialect(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public DbCommand BuildUpsertCommand(ISqlProjection projection, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap)
        {
            var columns = string.Join(", ", eventPropertyMap.Select(x => x.Value.Name));
            var parameterNames = string.Join(", ", eventPropertyMap.Select(x => $"@{x.Key.Name}"));

            var commandText = $@"
INSERT OR REPLACE INTO {projection.TableName} (
{columns}
)
VALUES (
{parameterNames}
)";

            var command = new SQLiteCommand(commandText);
            return command;
        }

        public DbCommand BuildDeleteCommand(ISqlProjection projection, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap)
        {
            throw new NotImplementedException();
        }

        public void BindParameters<TEvent>(DbCommand command, TEvent @event, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap)
        {
            foreach (var (propertyInfo, sqlColumnDefinition) in eventPropertyMap)
            {
                // TODO. We can do better than reflection here.
                var value = propertyInfo.GetValue(@event);

                var parameter = new SQLiteParameter($"@{sqlColumnDefinition.Name}", sqlColumnDefinition.DataType) {Value = value};
                command.Parameters.Add(parameter);
            }
        }
    }
}
