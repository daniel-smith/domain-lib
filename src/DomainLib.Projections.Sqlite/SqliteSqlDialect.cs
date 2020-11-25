using DomainLib.Projections.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace DomainLib.Projections.Sqlite
{
    public class SqliteSqlDialect : ISqlDialect
    {
        public DbCommand BuildUpsertCommand(ISqlProjection projection, Dictionary<PropertyInfo, SqlColumnDefinition> eventPropertyMap)
        {
            var updateFields = string.Join(", ",
                                           eventPropertyMap.Where(x => !x.Value.IsInPrimaryKey)
                                                           .Select(x => $"{x.Value.Name} = @{x.Key.Name}"));
            var primaryKeyFields = string.Join(", ",
                                               eventPropertyMap.Where(x => x.Value.IsInPrimaryKey)
                                                               .Select(x => $"{x.Value.Name} = @{x.Key.Name}"));
            var columns = string.Join(", ", eventPropertyMap.Select(x => x.Value.Name));
            var columnValues = string.Join(", ", eventPropertyMap.Select(x => $"{x.Value.Name} = @{x.Key.Name}"));

            var updateSql = $@"
UPDATE {projection.TableName}
SET
{updateFields}
WHERE 
{primaryKeyFields};";

            var insertSql = $@"
INSERT INTO {projection.TableName} (
{columns}
)
VALUES (
{columnValues}
)";

            var commandText = string.IsNullOrEmpty(updateFields) ? 
                                  $"{insertSql}; " : 
                                  $"{updateSql} {insertSql} WHERE changes() = 0; ";

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

                var parameter = new SQLiteParameter(sqlColumnDefinition.DataType, value);
                command.Parameters.Add(parameter);
            }
        }
    }
}
