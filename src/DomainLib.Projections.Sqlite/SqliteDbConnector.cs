using DomainLib.Common;
using DomainLib.Projections.Sql;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SQLite;

namespace DomainLib.Projections.Sqlite
{
    public sealed class SqliteDbConnector : IDbConnector
    {
        public static readonly ILogger<SqliteDbConnector> Log = Logger.CreateFor<SqliteDbConnector>();

        private readonly string _connectionString;

        public SqliteDbConnector(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public void BindParameters<TEvent>(IDbCommand command,
                                           TEvent @event,
                                           SqlColumnDefinitions columnDefinitions,
                                           ISqlParameterBindingMap<TEvent> parameterBindingMap)
        {
            try
            {
                foreach (var (name, value) in parameterBindingMap.GetParameterNamesAndValues(@event))
                {
                    var sqlColumnDefinition = columnDefinitions[name];
                    var parameter = new SQLiteParameter($"@{sqlColumnDefinition.Name}", sqlColumnDefinition.DataType)
                        {Value = value};
                    command.Parameters.Add(parameter);
                }
            }
            catch (Exception ex)
            {
                Log.LogCritical(ex,
                                "Unknown exception occurred while trying to bind parameters for event {EventName} {Event}",
                                @event.GetType().FullName,
                                @event);
                throw;
            }
        }
    }
}
