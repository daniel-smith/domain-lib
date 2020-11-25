using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public interface ISqlProjection
    {
        string TableName { get; }
        SqlColumnDefinitions Columns { get; }
        Task CustomCommandAsync(string sqlCommand);
    }
}