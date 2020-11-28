using System;
using DomainLib.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DomainLib.Projections.Sql
{
    public class SqlContext : IContext
    {
        private static readonly ILogger<SqlContext> Log = Logger.CreateFor<SqlContext>();
        private readonly IDbConnector _connector;
        private readonly HashSet<ISqlProjection> _projections = new HashSet<ISqlProjection>();
        private readonly SqlContextSettings _settings;

        private readonly StringBuilder _schemaStringBuilder = new StringBuilder();
        private IDbTransaction _activeTransaction;
        private bool _isProcessingLiveEvents;

        public SqlContext(IDbConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _settings = connector.ContextSettings;
            Connection = connector.CreateConnection();
        }

        public DbConnection Connection { get; }

        public async Task OnSubscribing()
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    await Connection.OpenAsync();
                    await CreateSchema();
                }

                if (_settings.UseTransactionBeforeCaughtUp)
                {
                    _activeTransaction = await Connection.BeginTransactionAsync();
                }

                _isProcessingLiveEvents = false;
            }
            catch (Exception ex)
            {
                Log.LogCritical(ex, "Exception occurred attempting to handle subscribing to event stream");
                throw;
            }
        }

        public Task OnCaughtUp()
        {
            try
            {
                if (_settings.UseTransactionBeforeCaughtUp)
                {
                    if (_activeTransaction != null)
                    {
                        _activeTransaction.Commit();
                        _activeTransaction = null;
                    }
                    else
                    {
                        Log.LogWarning("Caught up to live event stream, but no transaction was found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogCritical(ex, "Exception occurred attempting to handle live event stream starting");
                throw;
            }

            _isProcessingLiveEvents = true;
            return Task.CompletedTask;
        }

        public async Task OnBeforeHandleEvent()
        {
            if (_isProcessingLiveEvents)
            {
                if (_settings.HandleLiveEventsInTransaction)
                {
                    _activeTransaction = await Connection.BeginTransactionAsync();
                }
            }
        }

        public Task OnAfterHandleEvent()
        {
            if (_isProcessingLiveEvents)
            {
                if (_settings.HandleLiveEventsInTransaction)
                {
                    if (_activeTransaction != null)
                    {
                        _activeTransaction.Commit();
                        _activeTransaction = null;
                    }
                    else
                    {
                        Log.LogWarning("Expected to be in a transaction when handling event, but none was found.");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public void RegisterProjection(ISqlProjection projection)
        {
            if (_projections.Add(projection))
            {
                var createTableSql = string.IsNullOrEmpty(projection.CustomCreateTableSql)
                                         ? _connector.BuildCreateTableSql(projection.TableName, projection.Columns.Values)
                                         : projection.CustomCreateTableSql;

                createTableSql = string.Concat(createTableSql, " ", projection.AfterCreateTableSql, " ");

                _schemaStringBuilder.Append(createTableSql);
            }
        }

        private async Task CreateSchema()
        {
            try
            {
                var createSchemaCommand = Connection.CreateCommand();
            
                createSchemaCommand.CommandText = _schemaStringBuilder.ToString();
                await createSchemaCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.LogCritical(ex, "Unable to build SQL table schema");
                throw;
            }
        }
    }
}