using System;

namespace DomainLib.Routing
{
    public class CommandRegistrations<TCommandBase, TEventBase>
    {
        public CommandRoutes<TCommandBase, TEventBase> Routes { get; } = new CommandRoutes<TCommandBase, TEventBase>();
        public Action<TCommandBase> PreCommandHook { get; internal set; }
        public Action<TCommandBase> PostCommandHook { get; internal set; }
    }
}