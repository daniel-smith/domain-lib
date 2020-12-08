using System.Collections.Concurrent;

namespace DomainLib.Projections.Sql
{
    public static class SqlContextProvider
    {
        private static readonly ConcurrentDictionary<(IDbConnector, ISqlDialect), SqlContext> SqlContexts = new();

        public static SqlContext GetOrCreateContext(IDbConnector connector, ISqlDialect sqlDialect)
        {
            return SqlContexts.GetOrAdd((connector, sqlDialect),
                                        tuple => new SqlContext(tuple.Item1, tuple.Item2));
        }
    }
}