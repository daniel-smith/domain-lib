namespace DomainLib.Projections.Sql
{
    public interface ISqlProjection
    {
        string CreateSchemaSql { get; }
        string TableName { get; }
        SqlColumnDefinitions Columns { get; }
    }
}