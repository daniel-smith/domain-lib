using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public interface IEventPublisher<TEventBase>
    {
        Task StartAsync(Func<EventNotification<TEventBase>, Task> onEvent);
        void Stop();
    }
}