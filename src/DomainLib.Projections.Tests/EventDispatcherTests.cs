using System;
using System.Data;
using System.Threading.Tasks;
using DomainLib.Aggregates;
using DomainLib.Projections.Sql;
using DomainLib.Projections.Sqlite;
using DomainLib.Serialization.Json;
using NUnit.Framework;

namespace DomainLib.Projections.Tests
{
    [TestFixture]
    public class EventDispatcherTests
    {
        [Test]
        public async Task DoStuff()
        {
            var scenario = new SqlProjectionScenario();

            await scenario.Dispatcher.StartAsync();

            await scenario.Publisher.SendEvent(new TestEvent1(1, 10), TestEvent1.Name);


        }
    }

    public class SqlProjectionScenario
    {
        public FakeJsonEventPublisher Publisher = new();
        public EventDispatcher<object> Dispatcher;

        public SqlProjectionScenario()
        {
            CreateDispatcher();
        }

        public void CreateDispatcher()
        {
            var registryBuilder = new ProjectionRegistryBuilder();
            var projection = new FakeSqlProjection();

            registryBuilder.Event<TestEvent1>()
                           .FromName(TestEvent1.Name)
                           .ToSqlProjection(projection)
                           .ParameterMappings(("Col1", e => e.Id),
                                              ("Col2", e => e.Value))
                           .ExecutesUpsert();

            registryBuilder.Event<TestEvent2>()
                           .FromNames(TestEvent2.Name, TestEvent2.OtherName)
                           .ToSqlProjection(projection)
                           .ParameterMappings(("Col1", e => e.Id),
                                              ("Col3", e => e.Data))
                           .ExecutesUpsert();

            var registry = registryBuilder.Build();
            var serializer = new JsonEventSerializer(new EventNameMap());
            var dispatcherConfig = EventDispatcherConfiguration.ReadModelDefaults with { ProjectionHandlerTimeout =
                                       TimeSpan.FromHours(2)};

            var dispatcher = new EventDispatcher<object>(Publisher,
                                                         registry.EventProjectionMap,
                                                         registry.EventContextMap,
                                                         serializer,
                                                         registry.EventNameMap,
                                                         dispatcherConfig);

            Dispatcher = dispatcher;
        }
    }

    public class TestEvent1
    {
        public const string Name = "TestEvent1";

        public TestEvent1(int id, int value)
        {
            Value = value;
            Id = id;
        }

        public int Id { get; }
        public int Value { get; }
    }

    public class TestEvent2
    {
        public const string Name = "TestEvent2";
        public const string OtherName = "TestEvent2_OtherName";

        public TestEvent2(int id, string data)
        {
            Data = data;
            Id = id;
        }

        public int Id { get; }
        public string Data { get; }
    }

    public class FakeSqlProjection : ISqlProjection
    {
        public IDbConnector DbConnector { get; } = new FakeDbConnector();
        public ISqlDialect SqlDialect { get; } = new SqliteSqlDialect();
        public string TableName { get; } = "MyTable";

        public SqlColumnDefinitions Columns { get; } = new SqlColumnDefinitions
        {
            {"Col1", new SqlColumnDefinitionBuilder().Name("Col1").Type(DbType.Int32).PrimaryKey().Build()},
            {"Col2", new SqlColumnDefinitionBuilder().Name("Col2").Type(DbType.Int32).Build()},
            {"Col3", new SqlColumnDefinitionBuilder().Name("Col3").Type(DbType.String).Build()},
        };
    }
}
