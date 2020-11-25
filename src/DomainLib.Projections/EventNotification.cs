namespace DomainLib.Projections
{
    public readonly struct EventNotification<TEventBase>
    {
        internal EventNotification(EventNotificationKind notificationKind, TEventBase @event)
        {
            NotificationKind = notificationKind;
            Event = @event;
        }

        private EventNotificationKind NotificationKind { get; }
        private TEventBase Event { get; }
    }

    public static class EventNotification
    {
        public static EventNotification<TEventBase> CaughtUp<TEventBase>()
        {
            return new EventNotification<TEventBase>(EventNotificationKind.CaughtUpNotification, default);
        }

        public static EventNotification<TEventBase> FromEvent<TEventBase>(TEventBase @event)
        {
            return new EventNotification<TEventBase>(EventNotificationKind.Event, @event);
        }
    }
}