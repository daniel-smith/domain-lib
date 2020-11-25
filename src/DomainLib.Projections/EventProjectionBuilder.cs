namespace DomainLib.Projections
{
    public class EventProjectionBuilder<TEvent>
    {
        public EventProjectionBuilder<TEvent> FromName(string name)
        {
            return this;
        }

        public EventProjectionBuilder<TEvent> FromNames(params string[] names)
        {
            return this;
        }

        public void RegisterEventProjectionAction()
        {

        }
    }
}