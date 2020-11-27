﻿using System;
using DomainLib.EventStore.Testing;
using DomainLib.Projections;
using DomainLib.Projections.EventStore;
using DomainLib.Projections.Sql;
using DomainLib.Projections.Sqlite;
using DomainLib.Serialization.Json;
using NUnit.Framework;
using Shopping.Domain.Events;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DomainLib.Aggregates;
using DomainLib.Aggregates.Registration;
using DomainLib.Persistence;
using DomainLib.Persistence.EventStore;
using EventStore.ClientAPI;
using Shopping.Domain.Aggregates;
using Shopping.Domain.Commands;

namespace Shopping.Infrastructure.Tests
{
    [TestFixture]
    public class ShoppingCartReadModelTests : EmbeddedEventStoreTest
    {
        [Test]
        public async Task ReadModelIsBuilt()
        {
            var projectionRegistryBuilder = new ProjectionRegistryBuilder();

            ShoppingCartSummarySqlProjection.Register(projectionRegistryBuilder);

            var registry = projectionRegistryBuilder.Build();

            // TODO: EventNameMap isn't actually used here as we deserialize differently on the read side.
            // We need to fix this so that we don't have to pass an empty instance in
            var serializer = new JsonEventSerializer(new EventNameMap());
            var eventPublisher = new EventStoreEventPublisher<IDomainEvent>(EventStoreConnection, serializer, registry.EventNameMap);

            var eventStream = new EventStream<IDomainEvent>(eventPublisher, registry.EventProjectionMap, registry.EventContextMap);

            await eventStream.StartAsync();

            await WriteEventsToStream();

            await Task.Delay(1000);
        }

        private async Task WriteEventsToStream()
        {
            // TODO: Copied from ShoppingCartInfrastructureTests for the moment.
            // We should refactor this to allow better sharing

            var registryBuilder = AggregateRegistryBuilder.Create<object, IDomainEvent>();
            ShoppingCartFunctions.Register(registryBuilder);
            var shoppingCartId = Guid.NewGuid(); // This could come from a sequence, or could be the customer's ID.

            var aggregateRegistry = registryBuilder.Build();

            // Execute the first command.
            var initialState = new ShoppingCartState();

            var command1 = new AddItemToShoppingCart(shoppingCartId, Guid.NewGuid(), "First Item");
            var (newState1, events1) = aggregateRegistry.CommandDispatcher.ImmutableDispatch(initialState, command1);

            // Execute the second command to the result of the first command.
            var command2 = new AddItemToShoppingCart(shoppingCartId, Guid.NewGuid(), "Second Item");
            var (newState2, events2) = aggregateRegistry.CommandDispatcher.ImmutableDispatch(newState1, command2);

            Assert.That(newState2.Id.HasValue, "Expected ShoppingCart ID to be set");

            var eventsToPersist = events1.Concat(events2).ToList();

            var serializer = new JsonEventSerializer(aggregateRegistry.EventNameMap);
            var eventsRepository = new EventStoreEventsRepository(EventStoreConnection, serializer);
            var snapshotRepository = new EventStoreSnapshotRepository(EventStoreConnection, serializer);

            var aggregateRepository = new AggregateRepository<IDomainEvent>(eventsRepository,
                                                                            snapshotRepository,
                                                                            aggregateRegistry.EventDispatcher,
                                                                            aggregateRegistry.AggregateMetadataMap);

            var nextEventVersion = await aggregateRepository.SaveAggregate<ShoppingCartState>(newState2.Id.ToString(),
                                                                                              ExpectedVersion.NoStream,
                                                                                              eventsToPersist);
            var expectedNextEventVersion = eventsToPersist.Count - 1;

            Assert.That(nextEventVersion, Is.EqualTo(expectedNextEventVersion));
        }
    }

    public class ShoppingCartSummarySqlProjection : ISqlProjection
    {
        public static void Register(ProjectionRegistryBuilder builder)
        {
            var shoppingCartSummary = new ShoppingCartSummarySqlProjection();
            var sqliteDialect = new SqliteSqlDialect("Data Source=test.db; Version=3;Pooling=True;Max Pool Size=100;");
            
            builder.Event<ItemAddedToShoppingCart>()
                   .FromName(ItemAddedToShoppingCart.EventName)
                   .ToSqlProjection(shoppingCartSummary)
                   .UsingDialect(sqliteDialect)
                   .PerformUpsert();
        }

        public string TableName { get; } = "ShoppingCartSummary";

        public SqlColumnDefinitions Columns { get; } = new()
        {
            {nameof(ItemAddedToShoppingCart.Id), new SqlColumnDefinition("Id", DbType.String, true, false)},
            {nameof(ItemAddedToShoppingCart.CartId), new SqlColumnDefinition("CartId", DbType.String, false, true)},
            {nameof(ItemAddedToShoppingCart.Item), new SqlColumnDefinition("Item", DbType.String, false, true)},
        };
    }
}