using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public class SqlProjectionBuilder<TEvent, TSqlProjection> where TSqlProjection : ISqlProjection
    {
        private readonly EventProjectionBuilder<TEvent> _builder;
        private readonly TSqlProjection _sqlProjection;
        private readonly ISqlDialect _dialect;
        private readonly SqlContext _context;

        public SqlProjectionBuilder(EventProjectionBuilder<TEvent> builder, TSqlProjection sqlProjection) 
        {
            _builder = builder;
            _sqlProjection = sqlProjection;
            _dialect = sqlProjection.SqlDialect;
            _context = SqlContextProvider.GetOrCreateContext(sqlProjection.SqlDialect);
            _context.RegisterProjection(_sqlProjection);
            _builder.RegisterContextForEvent(_context);
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformUpsert()
        {
            var eventPropertyMap = BuildEventPropertyMap();
            var doUpsert = BuildExecuteNonQueryFunc(eventPropertyMap,
                                                    map => _dialect.BuildUpsertCommand(_sqlProjection, map));
            _builder.RegisterEventProjectionFunc<TSqlProjection>(doUpsert);

            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformDelete()
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

            var doDelete = BuildExecuteNonQueryFunc(eventPropertyMap,
                                                    map => _dialect.BuildDeleteCommand(_sqlProjection, map));
            _builder.RegisterEventProjectionFunc<TSqlProjection>(doDelete);

            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformCustomCommand(Func<TEvent, string> sqlCommand)
        {
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
                _dialect.BindParameters(command, @event, sqlColumnDefinitions);
                var rows = await command.ExecuteNonQueryAsync();
            };

        }
    }
}