using System.Data;

namespace DomainLib.Projections.Sql.Tests.Fakes
{
    public class FakeDbConnector : IDbConnector
    {
        public FakeDbConnector(SqlContextSettings contextSettings = null)
        {
            ContextSettings = contextSettings ?? SqlContextSettings.Default;
        }

        public IDbConnection CreateConnection()
        {
            Connection = new FakeDbConnection();
            return Connection;
        }

        public void BindParameters<TEvent>(IDbCommand command,
                                           TEvent @event,
                                           SqlColumnDefinitions columnDefinitions,
                                           ISqlParameterBindingMap<TEvent> parameterBindingMap)
        {
        }

        public SqlContextSettings ContextSettings { get; }
        public FakeDbConnection Connection { get; private set; }

    }
}
