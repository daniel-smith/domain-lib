using System;
using System.Threading.Tasks;

namespace DomainLib.Projections.EventStore
{
    public class AcknowledgingEventStoreEventPublisher<TEventBase> : IEventPublisher<TEventBase>
    {
        public Task StartAsync(Action<EventNotification<TEventBase>> onEvent)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
