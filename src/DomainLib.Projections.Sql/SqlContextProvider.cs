using System;
using System.Collections.Concurrent;

namespace DomainLib.Projections.Sql
{
    public static class SqlContextProvider
    {
        private static readonly ConcurrentDictionary<SqlContextKey, SqlContext> SqlContexts = new();

        public static SqlContext GetOrCreateContext(IDbConnector connector, ISqlDialect sqlDialect)
        {
            return SqlContexts.GetOrAdd(new SqlContextKey(connector, sqlDialect),
                                        key => new SqlContext(key.DbConnector, key.SqlDialect));
        }

        private readonly struct SqlContextKey : IEquatable<SqlContextKey>
        {
            private readonly Type _dbConnectorType;
            private readonly Type _sqlDialectType;

            public SqlContextKey(IDbConnector dbConnector, ISqlDialect sqlDialect)
            {
                DbConnector = dbConnector;
                SqlDialect = sqlDialect;
                _dbConnectorType = dbConnector.GetType();
                _sqlDialectType = sqlDialect.GetType();
            }

            public IDbConnector DbConnector { get; }
            public ISqlDialect SqlDialect { get; }

            public bool Equals(SqlContextKey other)
            {
                return _dbConnectorType == other._dbConnectorType && _sqlDialectType == other._sqlDialectType;
            }

            public override bool Equals(object obj)
            {
                return obj is SqlContextKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_dbConnectorType, _sqlDialectType);
            }

            public static bool operator ==(SqlContextKey left, SqlContextKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SqlContextKey left, SqlContextKey right)
            {
                return !left.Equals(right);
            }
        }
    }
}