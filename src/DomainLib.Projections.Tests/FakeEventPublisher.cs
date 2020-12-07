using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DomainLib.Projections.Tests
{
    public class FakeJsonEventPublisher : IEventPublisher<byte[]>
    {
        private Func<EventNotification<byte[]>, Task> _onEvent;
        public bool IsStarted { get; private set; }

        public Task StartAsync(Func<EventNotification<byte[]>, Task> onEvent)
        {
            _onEvent = onEvent;
            IsStarted = true;
            return Task.CompletedTask;
        }

        public void Stop()
        {
            IsStarted = false;
        }

        public async Task SendEvent(object @event, string eventType, Guid? eventId = null)
        {
            var byteArray = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            await _onEvent(EventNotification.FromEvent(byteArray, eventType, eventId ?? Guid.NewGuid()));
        }

        public async Task SendCaughtUp()
        {
            await _onEvent(EventNotification.CaughtUp<byte[]>());
        }
    }

}