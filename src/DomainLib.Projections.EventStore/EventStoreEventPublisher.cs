using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLib.Serialization;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace DomainLib.Projections.EventStore
{
    public class EventStoreEventPublisher<TEventBase> : IEventPublisher<TEventBase>, IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _serializer;
        private readonly IProjectionEventNameMap _projectionEventNameMap;
        private Func<EventNotification<TEventBase>, Task> _onEvent;
        private EventStoreCatchUpSubscription _subscription;

        public EventStoreEventPublisher(IEventStoreConnection connection, IEventSerializer serializer, IProjectionEventNameMap projectionEventNameMap)
        {
            _connection = connection;
            _serializer = serializer;
            _projectionEventNameMap = projectionEventNameMap;
        }

        public  Task StartAsync(Func<EventNotification<TEventBase>, Task> onEvent)
        {
            _onEvent = onEvent;
            var settings = CatchUpSubscriptionFilteredSettings.Default;
            _subscription = _connection.FilteredSubscribeToAllFrom(Position.Start,
                                                                   Filter.ExcludeSystemEvents,
                                                                   settings,
                                                                   HandleEvent,
                                                                   OnLiveProcessingStarted,
                                                                   OnSubscriptionDropped,
                                                                   new UserCredentials("admin", "changeit"));

            return Task.CompletedTask;
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _subscription.Stop();
        }

        private async Task HandleEvent(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var tasks = new List<Task>();

            foreach (var type in _projectionEventNameMap.GetClrTypesForEventName(resolvedEvent.Event.EventType))
            {
                var @event = _serializer.DeserializeEvent<TEventBase>(resolvedEvent.Event.Data, resolvedEvent.Event.EventType, type);
                var notification = EventNotification.FromEvent(@event);

                tasks.Add(_onEvent(notification));
            }

            await Task.WhenAll(tasks);
        }

        private void OnLiveProcessingStarted(EventStoreCatchUpSubscription arg1)
        {
            _onEvent(EventNotification.CaughtUp<TEventBase>());
        }

        private void OnSubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception exception)
        {
            // TODO: Need to handle this
        }
    }
}