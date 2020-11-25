using System;
using System.Threading.Tasks;
using DomainLib.Serialization;
using EventStore.ClientAPI;

namespace DomainLib.Projections.EventStore
{
    public class EventStoreEventPublisher<TEventBase> : IEventPublisher<TEventBase>, IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _serializer;
        private Action<EventNotification<TEventBase>> _onEventAction;
        private EventStoreSubscription _subscription;

        public EventStoreEventPublisher(IEventStoreConnection connection, IEventSerializer serializer)
        {
            _connection = connection;
            _serializer = serializer;
        }

        public async Task StartAsync(Action<EventNotification<TEventBase>> onEventAction)
        {
            _onEventAction = onEventAction;
            _subscription = await _connection.FilteredSubscribeToAllAsync(false, Filter.ExcludeSystemEvents, HandleEvent, OnSubscriptionDropped);
        }

        public void Stop()
        {
            Dispose();
        }

        private Task HandleEvent(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var @event = _serializer.DeserializeEvent<TEventBase>(resolvedEvent.Event.Data, resolvedEvent.Event.EventType);
            var notification = EventNotification.FromEvent(@event);

             _onEventAction(notification);

            // TODO: See if we can improve this. The FilteredSubscribeToAllAsync method expects
            // an async handle method. Maybe we should have async deserialize methods to go
            // alongside the sync ones?
            return Task.CompletedTask;
        }

        private void OnSubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason reason, Exception exception)
        {
            // TODO: Need to handle this
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}