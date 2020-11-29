using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public sealed class ProjectionRegistryBuilder
    {
        private readonly EventProjectionMap _eventProjectionMap = new EventProjectionMap();
        private readonly ProjectionEventNameMap _eventNameMap = new ProjectionEventNameMap();
        private readonly EventContextMap _eventContextMap = new EventContextMap();

        public EventProjectionBuilder<TEvent> Event<TEvent>()
        {
            return new EventProjectionBuilder<TEvent>(this);
        }

        internal void RegisterEventProjectionFunc<TEvent, TProjection>(Func<TEvent, Task> projectEvent)
        {
            if (projectEvent == null) throw new ArgumentNullException(nameof(projectEvent));
            _eventProjectionMap.AddProjectionFunc(typeof(TEvent), typeof(TProjection), @event => projectEvent((TEvent)@event));
        }

        internal void RegisterEventName<TEvent>(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _eventNameMap.RegisterTypeForEventName<TEvent>(name);
        }

        internal void RegisterContextForEvent<TEvent>(IContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _eventContextMap.RegisterContextForEvent<TEvent>(context);
        }

        public ProjectionRegistry Build()
        {
            return new ProjectionRegistry(_eventProjectionMap, _eventContextMap, _eventNameMap);
        }
    }
}