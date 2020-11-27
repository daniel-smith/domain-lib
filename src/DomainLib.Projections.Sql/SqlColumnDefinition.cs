﻿using System;
using System.Data;

namespace DomainLib.Projections.Sql
{
    public class SqlColumnDefinition
    {
        public SqlColumnDefinition(string name, DbType dataType, bool isInPrimaryKey, bool isNullable)
        {
            if (isInPrimaryKey && isNullable)
            {
                throw new ArgumentException("Column must not be nullable if it is in the primary key");
            }

            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataType = dataType;
            IsInPrimaryKey = isInPrimaryKey;
            IsNullable = isNullable;
        }

        public string Name { get; }
        public DbType DataType { get; }
        public bool IsInPrimaryKey { get; }
        public bool IsNullable { get; }
    }
}