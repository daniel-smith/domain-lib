using System;
using System.Collections.Generic;

namespace DomainLib.Routing
{
    public class CommandRoutes<TCommandBase, TEventBase> : Dictionary<(Type, Type), ApplyCommand<object, TCommandBase, TEventBase>>  { }
}