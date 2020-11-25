namespace DomainLib.Projections
{
    public class ProjectionRegistryBuilder
    {
        public EventProjectionBuilder<TEvent> Event<TEvent>()
        {
            return new EventProjectionBuilder<TEvent>();
        }
    }
}