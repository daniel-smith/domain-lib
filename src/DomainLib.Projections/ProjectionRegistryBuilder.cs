using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLib.Aggregates;

namespace DomainLib.Projections
{
    public class ProjectionRegistryBuilder
    {
        private readonly EventProjectionMap _eventProjectionMap = new EventProjectionMap();
        private readonly EventNameMap _eventNameMap = new EventNameMap();
        private readonly Dictionary<Type, IContext> _eventContextMap = new Dictionary<Type, IContext>();

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
            _eventNameMap.RegisterEvent<TEvent>(name);
        }

        internal void RegisterContextForEvent<TEvent>(IContext context)
        {
            _eventContextMap[typeof(TEvent)] = context;
        }

        public ProjectionRegistry Build()
        {
            return new ProjectionRegistry(_eventProjectionMap, _eventContextMap, _eventNameMap);
        }
    }
}