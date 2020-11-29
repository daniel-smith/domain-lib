using DomainLib.Projections.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;

namespace DomainLib.Projections.Sqlite
{
    public sealed class SqliteDbConnector : IDbConnector
    {
        private static readonly string SqlValueSeparator = $", {Environment.NewLine}";
        private static readonly string SqlPredicateSeparator = $" AND{Environment.NewLine}";
        private readonly string _connectionString;

        public SqliteDbConnector(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public string BuildCreateTableSql(string tableName, IEnumerable<SqlColumnDefinition> columnDefinitions)
        {
            var columnStrings = new List<string>();

            foreach (var column in columnDefinitions)
            {
                var nullableText = column.IsNullable ? "NULL" : "NOT NULL";
                var primaryKeyText = column.IsInPrimaryKey ? "PRIMARY KEY" : string.Empty;
                var columnString = string.Join(" ", column.Name, GetDataTypeName(column.DataType), nullableText, primaryKeyText);
                columnStrings.Add(columnString);
            }

            var columnsText = string.Join(SqlValueSeparator, columnStrings);


            var createTableSql = $@"
CREATE TABLE IF NOT EXISTS {tableName} (
{columnsText}
);
";
           return createTableSql;
        }

        public DbCommand BuildUpsertCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap)
        {
            var columns = string.Join(SqlValueSeparator, eventPropertyMap.Select(x => x.Value.Name));
            var parameterNames = string.Join(SqlValueSeparator, eventPropertyMap.Select(x => $"@{x.Value.Name}"));

            var commandText = $@"
INSERT OR REPLACE INTO {projection.TableName} (
{columns}
)
VALUES (
{parameterNames}
);
";

            var command = new SQLiteCommand(commandText);
            return command;
        }

        public DbCommand BuildDeleteCommand(ISqlProjection projection, EventSqlColumnDefinitions eventPropertyMap)
        {
            var primaryKeyColumns = eventPropertyMap.Where(x => x.Value.IsInPrimaryKey)
                                                    .Select(x => $"{x.Value.Name} = @{x.Value.Name}");

            var primaryKeysSql = string.Join(SqlPredicateSeparator, primaryKeyColumns);

            var commandText = $@"
DELETE FROM {projection.TableName}
WHERE
{primaryKeysSql}
;
";
            var command = new SQLiteCommand(commandText);
            return command;
        }
        
        public void BindParameters<TEvent>(DbCommand command, TEvent @event, EventSqlColumnDefinitions eventPropertyMap)
        {
            foreach (var (propertyInfo, sqlColumnDefinition) in eventPropertyMap)
            {
                // TODO. We can do better than reflection here.
                var value = propertyInfo.GetValue(@event);

                var parameter = new SQLiteParameter($"@{sqlColumnDefinition.Name}", sqlColumnDefinition.DataType) {Value = value};
                command.Parameters.Add(parameter);
            }
        }

        private static string GetDataTypeName(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Double:
                case DbType.Single:
                case DbType.VarNumeric:
                    return "NUMERIC";
                case DbType.Binary:
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return "INTEGER";
                case DbType.Object:
                    return "BLOB";
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Currency:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Decimal:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Time:
                case DbType.Xml:
                    return "TEXT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null);
            }
        }
    }
}
