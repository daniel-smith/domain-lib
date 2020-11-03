using DomainLib.Routing;
using NUnit.Framework;
using System.Collections.Generic;

namespace DomainLib.Tests.Routing
{
    public class CommandHookTests
    {
        [Test]
        public void PreAndPostCommandHooksAreCalled()
        {
            var loggedMessages = new List<string>();
            
            var messageRegistry = MessageRegistry.Create<TestCommand, TestEvent>();
            messageRegistry.RegisterPreCommandHook(cmd => loggedMessages.Add($"Pre-command {cmd.Name}"));
            messageRegistry.RegisterPostCommandHook(cmd => loggedMessages.Add($"Post-command {cmd.Name}"));

            messageRegistry.RegisterAggregate<State>(x =>
            {
                x.Command<TestCommand>().RoutesTo((_, cmd) => new List<TestEvent> {new TestEvent(cmd.Name)});
                x.Event<TestEvent>().RoutesTo((state, e) =>
                {
                    loggedMessages.Add(e.Name);
                    return state;
                }).HasName("TestEvent");
            });

            messageRegistry.BuildCommandDispatcher().Dispatch(new State(), new TestCommand("My command"));

            Assert.That(loggedMessages, Is.EquivalentTo(new List<string>
            {
                "Pre-command My command",
                "My command",
                "Post-command My command"
            }));

        }

        public class State
        {
        }

        public class TestCommand
        {
            public TestCommand(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class TestEvent
        {
            public TestEvent(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}