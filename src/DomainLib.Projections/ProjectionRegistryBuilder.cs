using System;
using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public class ProjectionRegistryBuilder
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
            _eventProjectionMap.AddProjectionFunc(typeof(TEvent), typeof(TProjection), @event => projectEvent((TEvent)@event));
        }

        internal void RegisterEventName<TEvent>(string name)
        {
            _eventNameMap.RegisterTypeForEventName<TEvent>(name);
        }

        internal void RegisterContextForEvent<TEvent>(IContext context)
        {
            _eventContextMap.RegisterContextForEvent<TEvent>(context);
        }

        public ProjectionRegistry Build()
        {
            return new ProjectionRegistry(_eventProjectionMap, _eventContextMap, _eventNameMap);
        }
    }
}