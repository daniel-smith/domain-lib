namespace DomainLib.Projections
{
    public class ProjectionRegistry
    {
        public ProjectionRegistry(EventProjectionMap eventProjectionMap, EventContextMap eventContextMap, IProjectionEventNameMap eventNameMap)
        {
            EventProjectionMap = eventProjectionMap;
            EventContextMap = eventContextMap;
            EventNameMap = eventNameMap;
        }

        public EventProjectionMap EventProjectionMap { get; }
        public IProjectionEventNameMap EventNameMap { get; }
        public EventContextMap EventContextMap { get; }
    }
}