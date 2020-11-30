using System;

namespace DomainLib.Projections
{
    public readonly struct EventNotification<TEventBase>
    {
        internal EventNotification(EventNotificationKind notificationKind, TEventBase @event, Guid eventId)
        {
            NotificationKind = notificationKind;
            Event = @event;
            EventId = eventId;
        }

        public EventNotificationKind NotificationKind { get; }
        public TEventBase Event { get; }
        public Guid EventId { get; }
    }

    public static class EventNotification
    {
        public static EventNotification<TEventBase> CaughtUp<TEventBase>()
        {
            return new EventNotification<TEventBase>(EventNotificationKind.CaughtUpNotification, default, Guid.Empty);
        }

        public static EventNotification<TEventBase> FromEvent<TEventBase>(TEventBase @event, Guid eventId)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            return new EventNotification<TEventBase>(EventNotificationKind.Event, @event, eventId);
        }
    }
}