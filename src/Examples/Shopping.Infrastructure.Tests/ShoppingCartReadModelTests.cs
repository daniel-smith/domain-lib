using System;
using DomainLib.Projections;
using DomainLib.Projections.Sql;
using NUnit.Framework;
using Shopping.Domain.Events;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DomainLib.Projections.Sqlite;

namespace Shopping.Infrastructure.Tests
{
    [TestFixture]
    public class ShoppingCartReadModelTests
    {
        [Test]
        public void ReadModelIsBuilt()
        {
            var projectionRegistryBuilder = new ProjectionRegistryBuilder();

            ShoppingCartSummarySqlProjection.Register(projectionRegistryBuilder);
        }
    }

    public class ShoppingCartSummarySqlProjection : ISqlProjection
    {
        public static void Register(ProjectionRegistryBuilder builder)
        {
            var shoppingCartSummary = new ShoppingCartSummarySqlProjection();
            var sqliteDialect = new SqliteSqlDialect();

            builder.Event<ShoppingCartCreated>()
                   .FromName(ShoppingCartCreated.EventName)
                   .ToSqlProjection(shoppingCartSummary)
                   .UsingDialect(sqliteDialect)
                   .PerformUpsert();

            builder.Event<ItemAddedToShoppingCart>()
                   .FromName(ItemAddedToShoppingCart.EventName)
                   .ToSqlProjection(shoppingCartSummary)
                   .UsingDialect(sqliteDialect)
                   .PerformUpsert();
        }

        public string TableName { get; } = "ShoppingCartSummary";

        public SqlColumnDefinitions Columns { get; } = new()
        {
            {nameof(ShoppingCartCreated.Id), new SqlColumnDefinition("Id", DbType.String, true, false)},
            {nameof(ItemAddedToShoppingCart.Item), new SqlColumnDefinition("Item", DbType.String, false, true)},
        };

        public Task CustomCommandAsync(string sqlCommand)
        {
            throw new System.NotImplementedException();
        }
    }
}
