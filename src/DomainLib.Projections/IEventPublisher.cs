using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public interface IEventPublisher<TEventBase>
    {
        Task StartAsync(Action<EventNotification<TEventBase>> onEvent);
        void Stop();
    }
}