using System.Collections.Generic;
using System.Reflection;

namespace DomainLib.Projections.Sql
{
    public sealed class EventSqlColumnDefinitions : Dictionary<PropertyInfo, SqlColumnDefinition>
    {
    }
}