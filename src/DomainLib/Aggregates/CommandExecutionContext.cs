namespace DomainLib.Aggregates
{
    public static class CommandExecutionContext
    {
        public static CommandExecutionContext<TAggregateRoot, TDomainEventBase>
            Create<TAggregateRoot, TDomainEventBase>(
                TAggregateRoot aggregateRoot)
        {
            var commandResult = new CommandResult<TAggregateRoot, TDomainEventBase>(aggregateRoot);
            return new CommandExecutionContext<TAggregateRoot, TDomainEventBase>(commandResult);
        }
    }
    
    /// <summary>
    /// Applies state mutations to an aggregate root by routing the events that occur as part of executing a command
    /// to their appropriate "apply event" methods.
    /// </summary>
    public class CommandExecutionContext<TAggregateRoot, TDomainEventBase>
    {
        public CommandExecutionContext(
            CommandResult<TAggregateRoot, TDomainEventBase> commandResult)
        {
            Result = commandResult;
        }
        
        /// <summary>
        /// The result of executing a command on an aggregate root.
        /// </summary>
        public CommandResult<TAggregateRoot, TDomainEventBase> Result { get; }

        /// <summary>
        /// Applies an event to the aggregate root.
        /// </summary>
        public CommandExecutionContext<TAggregateRoot, TDomainEventBase> ApplyEvent<TDomainEvent>(TDomainEvent @event)
            where TDomainEvent : TDomainEventBase
        {
            var newState = EventRegistry.RouteEvent(Result.NewState, @event);
            var newResult = Result.WithNewState(newState, @event);
            return new CommandExecutionContext<TAggregateRoot, TDomainEventBase>(newResult);
        }
    }
}