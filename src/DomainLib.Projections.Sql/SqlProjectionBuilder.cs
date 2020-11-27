using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public class SqlProjectionBuilder<TEvent, TSqlProjection> where TSqlProjection : ISqlProjection
    {
        private readonly EventProjectionBuilder<TEvent> _builder;
        private readonly TSqlProjection _sqlProjection;
        private ISqlDialect _dialect;
        private SqlContext _context;

        public SqlProjectionBuilder(EventProjectionBuilder<TEvent> builder, TSqlProjection sqlProjection) 
        {
            _builder = builder;
            _sqlProjection = sqlProjection;
        }

        // TODO: Right now this needs to be called before PerformUpsert/PerformDelete, but the interface doesn't enforce this
        public SqlProjectionBuilder<TEvent, TSqlProjection> UsingDialect(ISqlDialect dialect)
        {
            _dialect = dialect;
            _context = SqlContextProvider.GetOrCreateContext(dialect);
            _context.AddSchema(_sqlProjection.CreateSchemaSql);
            _builder.RegisterContextForEvent(_context);
            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformUpsert()
        {
            var eventPropertyMap = new Dictionary<PropertyInfo, SqlColumnDefinition>();
            var eventProperties = typeof(TEvent).GetProperties();

            foreach (var (key, column) in _sqlProjection.Columns)
            {
                var propertyInfo = eventProperties.FirstOrDefault(x => x.Name == key);
                if (propertyInfo != null)
                {
                    eventPropertyMap.Add(propertyInfo, column);
                }
            }

            var doUpsert = BuildUpsertFunc(eventPropertyMap);
            _builder.RegisterEventProjectionFunc<TSqlProjection>(doUpsert);

            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformDelete()
        {
            return this;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> PerformCustomCommand(Func<TEvent, string> sqlCommand)
        {
            return this;
        }

        private Func<TEvent, Task> BuildUpsertFunc(Dictionary<PropertyInfo, SqlColumnDefinition> sqlColumnDefinitions)
        {
            var command = _dialect.BuildUpsertCommand(_sqlProjection, sqlColumnDefinitions);
            command.Connection = _context.Connection;

            return async @event =>
            {
                _dialect.BindParameters(command, @event, sqlColumnDefinitions);
                var rows = await command.ExecuteNonQueryAsync();
            };

        }
    }
}