﻿namespace DomainLib.Projections.Sql
{
    public interface ISqlProjection
    {
        ISqlDialect SqlDialect { get; }
        string CustomCreateTableSql => string.Empty;
        string AfterCreateTableSql => string.Empty;
        string TableName { get; }
        SqlColumnDefinitions Columns { get; }
    }
}