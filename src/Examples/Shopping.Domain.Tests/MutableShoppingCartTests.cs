using DomainLib.Aggregates.Registration;
using NUnit.Framework;
using Shopping.Domain.Aggregates;
using Shopping.Domain.Commands;
using Shopping.Domain.Events;

namespace Shopping.Domain.Tests
{
    [TestFixture]
    public class MutableShoppingCartTests
    {
        [Test]
        public void RoundTripTest()
        {
            var aggregateRegistryBuilder = AggregateRegistryBuilder.Create<object, IDomainEvent>();
            
            aggregateRegistryBuilder.Register<MutableShoppingCart>(reg =>
            {
                reg.Command<AddItemToShoppingCart>().RoutesTo((agg, cmd) => agg.Execute(cmd));
            });
        }
    }
}