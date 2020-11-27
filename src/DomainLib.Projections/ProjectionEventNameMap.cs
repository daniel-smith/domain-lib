using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainLib.Projections
{
    internal class ProjectionEventNameMap : IProjectionEventNameMap
    {
        private readonly Dictionary<string, IList<Type>> _eventNameMap = new Dictionary<string, IList<Type>>();

        public IEnumerable<Type> GetClrTypesForEventName(string eventName)
        {
            return _eventNameMap.TryGetValue(eventName, out var types) ? 
                       types : 
                       Enumerable.Empty<Type>();
        }

        public void RegisterTypeForEventName<TEvent>(string eventName)
        {
            if (_eventNameMap.TryGetValue(eventName, out var types))
            {
                types.Add(typeof(TEvent));
            }
            else
            {
                var typesList = new List<Type> {typeof(TEvent)};
                _eventNameMap.Add(eventName, typesList);
            }
        }
    }
}