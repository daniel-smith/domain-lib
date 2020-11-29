using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public sealed class EventStream<TEventBase>
    {
        private readonly IEventPublisher<TEventBase> _publisher;
        private readonly EventProjectionMap _projectionMap;
        private readonly EventContextMap _eventContextMap;

        public EventStream(IEventPublisher<TEventBase> publisher, EventProjectionMap projectionMap, EventContextMap eventContextMap)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _projectionMap = projectionMap ?? throw new ArgumentNullException(nameof(projectionMap));
            _eventContextMap = eventContextMap ?? throw new ArgumentNullException(nameof(eventContextMap));
        }

        public async Task StartAsync()
        {
            await ForAllContexts(c => c.OnSubscribing()).ConfigureAwait(false);
            await _publisher.StartAsync(HandleEventNotificationAsync).ConfigureAwait(false);
        }

        private async Task ForAllContexts(Func<IContext, Task> contextAction)
        {
            foreach (var context in _eventContextMap.GetAllContexts())
            {
                await contextAction(context).ConfigureAwait(false); 
            }
        }

        private async Task HandleEventNotificationAsync(EventNotification<TEventBase> notification)
        {
            switch (notification.NotificationKind)
            {
                case EventNotificationKind.Event:
                    await HandleEventAsync(notification.Event).ConfigureAwait(false);
                    break;
                case EventNotificationKind.CaughtUpNotification:
                    await ForAllContexts(c => c.OnCaughtUp()).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleEventAsync(TEventBase @event)
        {
            var eventType = @event.GetType();
            var contextsForEvent = _eventContextMap.GetContextsForEventType(eventType);

            var beforeEventActions = contextsForEvent.Select(c => c.OnBeforeHandleEvent());
            await Task.WhenAll(beforeEventActions).ConfigureAwait(false);

            
            if (_projectionMap.TryGetValue(eventType, out var projections))
            {
                //TODO: Investigate replacing this with an async enumerator
                var tasks = new List<Task>();

                foreach (var (_, executeAsync) in projections)
                {
                    tasks.Add(executeAsync(@event));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            var afterEventActions = contextsForEvent.Select(c => c.OnAfterHandleEvent());
            await Task.WhenAll(afterEventActions).ConfigureAwait(false);
        }
    }
}
