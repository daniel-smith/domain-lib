using System;
using System.Collections.Generic;

namespace DomainLib.Routing
{
    public class EventRoutes<TEventBase> : Dictionary<(Type, Type), ApplyEvent<object, TEventBase>> { }
}