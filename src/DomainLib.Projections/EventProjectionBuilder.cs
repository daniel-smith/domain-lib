using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public class EventProjectionBuilder<TEvent>
    {
        private readonly ProjectionRegistryBuilder _builder;

        public EventProjectionBuilder(ProjectionRegistryBuilder builder)
        {
            _builder = builder;
        }

        public EventProjectionBuilder<TEvent> FromName(string name)
        {
            _builder.RegisterEventName<TEvent>(name);
            return this;
        }

        public EventProjectionBuilder<TEvent> FromNames(params string[] names)
        {
            // TODO: As we can have multiple names that map to the same type on the read side
            // The standard EventNameMap isn't quite going to work.
            // We'll need a new implementation and maybe we should split up the interface IEventNameMap too
            return this;
        }

        public void RegisterEventProjectionFunc<TProjection>(Func<TEvent, Task> projectEvent)
        {
            _builder.RegisterEventProjectionFunc<TEvent, TProjection>(projectEvent);
        }

        public void RegisterContextForEvent(IContext context)
        {
            _builder.RegisterContextForEvent<TEvent>(context);
        }
    }
}