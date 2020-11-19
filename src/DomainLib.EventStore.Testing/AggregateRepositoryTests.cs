using DomainLib.Aggregates;
using DomainLib.Aggregates.Registration;
using DomainLib.Persistence;
using DomainLib.Persistence.EventStore;
using DomainLib.Serialization.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainLib.EventStore.Testing
{
    [TestFixture]
    public class AggregateRepositoryTests : EmbeddedEventStoreTest
    {
        private CommandDispatcher<TestCommand, TestEvent> _commandDispatcher;
        private AggregateRepository<TestEvent> _aggregateRepository;

        [Test]
        public async Task SnapshotLifeCycleTest()
        {
            var id = Guid.NewGuid();
            var initialState = new TestAggregateState(id, 0);

            var (state, eventsList) = DispatchCommandsToState(initialState, 100);
            
            var nextVersion = await _aggregateRepository.SaveAggregate<TestAggregateState>(id.ToString(), StreamVersion.NewStream, eventsList);
            await _aggregateRepository.SaveSnapshot(VersionedAggregateState.Create(state, nextVersion));

            var (_, newEventsList) = DispatchCommandsToState(state, 10);
            await _aggregateRepository.SaveAggregate<TestAggregateState>(id.ToString(), nextVersion, newEventsList);

            var aggregate1 = await _aggregateRepository.LoadAggregate(id.ToString(), initialState);
            Assert.That(aggregate1.AggregateState.TotalNumber, Is.EqualTo(110));

            await _aggregateRepository.SaveSnapshot(aggregate1);
            var aggregate2 = await _aggregateRepository.LoadAggregate(id.ToString(), initialState);

            Assert.That(aggregate2.AggregateState.TotalNumber, Is.EqualTo(110));
        }

        private (TestAggregateState state, IList<TestEvent> events) DispatchCommandsToState(TestAggregateState initialState, int commandCount)
        {
            var state = initialState;
            var commands = Enumerable.Range(1, commandCount).Select(_ => new TestCommand(1));
            var eventsList = new List<TestEvent>();

            foreach (var command in commands)
            {
                var result = _commandDispatcher.Dispatch(state, command);

                eventsList.AddRange(result.AppliedEvents);
                state = result.NewState;
            }

            return (state, eventsList);
        }

        [SetUp]
        public void TestSetUp()
        {
            var registryBuilder = AggregateRegistryBuilder.Create<TestCommand, TestEvent>();
            TestAggregateFunctions.Register(registryBuilder);

            var registry = registryBuilder.Build();

            var serializer = new JsonEventSerializer(registry.EventNameMap);

            _commandDispatcher = registry.CommandDispatcher;
            var snapshotRepository = new EventStoreSnapshotRepository(EventStoreConnection, serializer);
            var eventsRepository = new EventStoreEventsRepository(EventStoreConnection, serializer);

            _aggregateRepository = new AggregateRepository<TestEvent>(eventsRepository, snapshotRepository, registry.EventDispatcher, registry.AggregateMetadataMap);
        }
    }
}