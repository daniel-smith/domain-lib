using System;
using System.Data.Common;
using System.Linq;

namespace DomainLib.Projections.Sql
{
    public static class DbParameterCollectionExtension
    {
        public static string ToFormattedString(this DbParameterCollection collection)
        {
            return string.Join($", {Environment.NewLine}",
                               collection.Cast<DbParameter>()
                                         .Select(p => $"Name: {p.ParameterName}, " +
                                                      $"Type: {p.DbType}, Value: {p.Value}"));
        }
    }
}