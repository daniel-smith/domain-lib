using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public class EventStream<TEventBase>
    {
        private readonly IEventPublisher<TEventBase> _publisher;
        private readonly ProjectionRegistry _projectionRegistry;

        public EventStream(IEventPublisher<TEventBase> publisher, ProjectionRegistry projectionRegistry)
        {
            _publisher = publisher;
            _projectionRegistry = projectionRegistry;
        }

        public async Task StartAsync()
        {
            await _publisher.StartAsync(HandleEventNotification);
        }

        private void HandleEventNotification(EventNotification<TEventBase> notification)
        {

        }
    }
}
