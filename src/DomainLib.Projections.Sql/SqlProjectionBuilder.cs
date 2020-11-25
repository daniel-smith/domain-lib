using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DomainLib.Projections.Sql
{
    public class SqlProjectionBuilder<TEvent, TSqlProjection> where TSqlProjection : ISqlProjection
    {
        private readonly EventProjectionBuilder<TEvent> _builder;
        private readonly TSqlProjection _sqlProjection;
        private ISqlDialect _dialect;

        public SqlProjectionBuilder(EventProjectionBuilder<TEvent> builder, TSqlProjection sqlProjection) 
        {
            _builder = builder;
            _sqlProjection = sqlProjection;
        }

        public SqlProjectionBuilder<TEvent, TSqlProjection> UsingDialect(ISqlDialect dialect)
        {
            _dialect = dialect;
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

            var upsertCommand = _dialect.BuildUpsertCommand(_sqlProjection, eventPropertyMap);

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
    }
}