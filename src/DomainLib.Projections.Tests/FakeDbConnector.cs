using System.Data;
using DomainLib.Projections.Sql;
using NSubstitute;

namespace DomainLib.Projections.Tests
{
    public class FakeDbConnector : IDbConnector
    {
        public IDbConnection CreateConnection()
        {
            var substitute = Substitute.For<IDbConnection>();

            return substitute;
        }

        public void BindParameters<TEvent>(IDbCommand command,
                                           TEvent @event,
                                           SqlColumnDefinitions columnDefinitions,
                                           ISqlParameterBindingMap<TEvent> parameterBindingMap)
        {
        }
    }
}