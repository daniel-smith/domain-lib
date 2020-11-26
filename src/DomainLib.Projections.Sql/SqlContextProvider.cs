using System.Collections.Concurrent;

namespace DomainLib.Projections.Sql
{
    public class SqlContextProvider
    {
        private static readonly ConcurrentDictionary<ISqlDialect, SqlContext> SqlContexts = new ConcurrentDictionary<ISqlDialect, SqlContext>();

        public static SqlContext GetOrCreateContext(ISqlDialect dialect)
        {
            return SqlContexts.GetOrAdd(dialect,
                                 d =>
                                 {
                                     var connection = d.CreateConnection();
                                     return new SqlContext(connection);
                                 });
        }
    }
}