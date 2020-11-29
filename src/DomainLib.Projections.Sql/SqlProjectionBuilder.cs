using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public sealed class SqlProjectionBuilder<TEvent, TSqlProjection> where TSqlProjection : ISqlProjection
    {
        private readonly EventProjectionBuilder<TEvent> _builder;
        private readonly TSqlProjection _sqlProjection;
        private readonly IDbConnector _connector;
        private readonly SqlContext _context;

        public SqlProjectionBuilder(EventProjectionBuilder<TEvent> builder, TSqlProjection sqlProjection) 
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _sqlProjection = sqlProjection ?? throw new ArgumentNullException(nameof(sqlProjection));
            _connector = sqlProjection.DbConnector;
            _context = SqlContextProvider.GetOrCreateContext(sqlProjection.DbConnector);
            _context.RegisterProjection(_sqlProjection);
            _builder.RegisterContextForEvent(_context);
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> ExecutesUpsert()
        {
            var eventPropertyMap = BuildEventPropertyMap();
            var executeUpsert = BuildExecuteNonQueryFunc(eventPropertyMap,
                                                    map => _connector.BuildUpsertCommand(_sqlProjection, map));
            _builder.RegisterEventProjectionFunc<TSqlProjection>(executeUpsert);

            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> ExecutesDelete()
        {
            var eventPropertyMap = BuildEventPropertyMap();

            var eventColumnNames = eventPropertyMap.Values.Select(c => c.Name).ToList();

            if (!_sqlProjection.Columns.Where(c => c.Value.IsInPrimaryKey)
                               .All(pk => eventColumnNames.Contains(pk.Value.Name)))
            {
                throw new InvalidOperationException($"All primary key columns must be present in event. " +
                                                    $"{typeof(TEvent).FullName} cannot be used to delete " +
                                                    $"from table {_sqlProjection.TableName} as it can't identify " +
                                                    $"a single row. If you wish to delete multiple rows from this " +
                                                    $"event, you will need to use a custom command instead");
            }

            var executeDelete = BuildExecuteNonQueryFunc(eventPropertyMap,
                                                    map => _connector.BuildDeleteCommand(_sqlProjection, map));
            _builder.RegisterEventProjectionFunc<TSqlProjection>(executeDelete);

            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> ExecutesCustomSql(string sqlCommand)
        {
            var eventPropertyMap = BuildEventPropertyMap();
            var executeCustomSql = BuildExecuteNonQueryFunc(eventPropertyMap,
                                                            map =>
                                                            {
                                                                var command = _context.Connection.CreateCommand();
                                                                command.CommandText = sqlCommand;
                                                                return command;
                                                            });

            _builder.RegisterEventProjectionFunc<TSqlProjection>(executeCustomSql);

            return this;
        }

        private EventSqlColumnDefinitions BuildEventPropertyMap()
        {
            var eventPropertyMap = new EventSqlColumnDefinitions();
            var eventProperties = typeof(TEvent).GetProperties();

            foreach (var (key, column) in _sqlProjection.Columns)
            {
                var propertyInfo = eventProperties.FirstOrDefault(x => x.Name == key);
                if (propertyInfo != null)
                {
                    eventPropertyMap.Add(propertyInfo, column);
                }
            }

            return eventPropertyMap;
        }

        private Func<TEvent, Task> BuildExecuteNonQueryFunc(EventSqlColumnDefinitions sqlColumnDefinitions, Func<EventSqlColumnDefinitions, DbCommand> buildCommand)
        {
            var command = buildCommand(sqlColumnDefinitions);
            command.Connection = _context.Connection;

            return async @event =>
            {
                _connector.BindParameters(command, @event, sqlColumnDefinitions);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    Console.WriteLine("No rows affected!");
                }
            };

        }
    }
}