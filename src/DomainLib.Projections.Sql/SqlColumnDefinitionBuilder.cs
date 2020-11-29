using System;
using System.Data;

namespace DomainLib.Projections.Sql
{
    public sealed class SqlColumnDefinitionBuilder
    {
        private string _name;
        private DbType _dataType = DbType.String;
        private bool _isInPrimaryKey = false;
        private bool _isNullable = true;

        public SqlColumnDefinitionBuilder Name(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        public SqlColumnDefinitionBuilder Type(DbType dbType)
        {
            _dataType = dbType;
            return this;
        }

        public SqlColumnDefinitionBuilder NotNull()
        {
            _isNullable = false;
            return this;
        }

        public SqlColumnDefinitionBuilder Null()
        {
            _isNullable = true;
            return this;
        }

        public SqlColumnDefinitionBuilder PrimaryKey()
        {
            _isInPrimaryKey = true;
            _isNullable = false;
            return this;
        }

        public SqlColumnDefinition Build()
        {
            if (string.IsNullOrEmpty(_name))
            {
                throw new InvalidOperationException("Name must be supplied for a SQL Column");
            }

            return new SqlColumnDefinition(_name, _dataType, _isInPrimaryKey, _isNullable);
        }
    }
}