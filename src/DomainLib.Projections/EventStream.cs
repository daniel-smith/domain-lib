using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public class EventStream<TEventBase>
    {
        private readonly IEventPublisher<TEventBase> _publisher;
        private readonly EventProjectionMap _projectionMap;
        private readonly Dictionary<Type, IContext> _eventContextMap;

        public EventStream(IEventPublisher<TEventBase> publisher, EventProjectionMap projectionMap, Dictionary<Type, IContext> eventContextMap)
        {
            _publisher = publisher;
            _projectionMap = projectionMap;
            _eventContextMap = eventContextMap;
        }

        public async Task StartAsync()
        {
            // TODO: Right now, this is going to iterate over all events in the map, even if the context is the same
            foreach (var (key, context) in _eventContextMap)
            {
                await context.OnSubscribing();
            }

            await _publisher.StartAsync(HandleEventNotificationAsync);
        }

        private async Task HandleEventNotificationAsync(EventNotification<TEventBase> notification)
        {
            if (notification.NotificationKind == EventNotificationKind.Event)
            {
                var eventType = notification.Event.GetType();
                if (_projectionMap.TryGetValue(eventType, out var projections))
                {
                   //TODO: Investigate replacing this with an async enumerator
                   var tasks = new List<Task>();

                    foreach (var (_, executeAsync) in projections)
                    {
                        tasks.Add(executeAsync(notification.Event));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            
        }
    }
}
