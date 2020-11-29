using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public sealed class EventProjectionBuilder<TEvent>
    {
        private readonly ProjectionRegistryBuilder _builder;

        public EventProjectionBuilder(ProjectionRegistryBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public EventProjectionBuilder<TEvent> FromName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _builder.RegisterEventName<TEvent>(name);
            return this;
        }

        public EventProjectionBuilder<TEvent> FromNames(params string[] names)
        {
            foreach (var name in names)
            {
                _builder.RegisterEventName<TEvent>(name);
            }

            return this;
        }

        public void RegisterEventProjectionFunc<TProjection>(Func<TEvent, Task> projectEvent)
        {
            if (projectEvent == null) throw new ArgumentNullException(nameof(projectEvent));
            _builder.RegisterEventProjectionFunc<TEvent, TProjection>(projectEvent);
        }

        public void RegisterContextForEvent(IContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _builder.RegisterContextForEvent<TEvent>(context);
        }
    }
}