using System.Collections.Concurrent;

namespace DomainLib.Projections.Sql
{
    public class SqlContextProvider
    {
        private static readonly ConcurrentDictionary<IDbConnector, SqlContext> SqlContexts = new ConcurrentDictionary<IDbConnector, SqlContext>();

        public static SqlContext GetOrCreateContext(IDbConnector connector)
        {
            return SqlContexts.GetOrAdd(connector,
                                 d => new SqlContext(d));
        }
    }
}