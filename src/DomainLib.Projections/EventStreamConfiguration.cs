using System;

namespace DomainLib.Projections
{
    public class EventStreamConfiguration
    {
        public EventStreamConfiguration(TimeSpan projectionHandlerTimeout, bool continueAfterTimeout, bool continueAfterProjectionException)
        {
            ProjectionHandlerTimeout = projectionHandlerTimeout;
            ContinueAfterTimeout = continueAfterTimeout;
            ContinueAfterProjectionException = continueAfterProjectionException;
        }

        public static EventStreamConfiguration ReadModelDefaults =
            new EventStreamConfiguration(TimeSpan.FromSeconds(5), false, false);

        public static EventStreamConfiguration ProcessDefaults =
            new EventStreamConfiguration(TimeSpan.FromSeconds(5), true, true);

        public TimeSpan ProjectionHandlerTimeout { get; }
        public bool ContinueAfterTimeout { get; }
        public bool ContinueAfterProjectionException { get; }
    }
}