#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.Extensions;
using AsyncORM.interfaces;

#endregion

namespace AsyncORM
{
    public class QueryAsync : BaseDatabaseAsync, IQueryAsync
    {
        protected QueryAsync(string connectionString) : base(connectionString)
        {
            
        }

        protected QueryAsync()
        {
           if (String.IsNullOrEmpty(AsyncOrmConfig.ConnectionString))
                throw new ArgumentNullException("please setup global connectionstring use the following: AsyncOrmConfig.ConnectionString");
        }

        #region IQueryAsync Members

        public async Task<IEnumerable<dynamic>> ExecuteAsync(string query, object dbParams = null,
            IsolationLevel isolationLevel =
                IsolationLevel.ReadCommitted, int commandTimeout = 30,
            CancellationToken cancellationToken =
                default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (var comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType, query, commandTimeout, comm,
                                dbParams);
                            IEnumerable<dynamic> items;
                            using (
                                var reader =
                                    await
                                        comm.ExecuteReaderAsync(cancellationToken)
                                            .ConfigureAwait(AsyncOrmConfig.ConfigureAwait))
                            {
                                items =
                                    await
                                        reader.ToExpandoMultipleListAsync(cancellationToken)
                                            .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                            }

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException("OperationCanceled", cancellationToken);

                            trans.Commit();
                            return items.Count() == 1 ? items.ElementAt(0) : items;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<IEnumerable<T>> ExecuteAsync<T>(string query, object dbParams = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int commandTimeout = 30,
            CancellationToken cancellationToken =
                default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (var comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType, query, commandTimeout, comm,
                                dbParams);
                            IEnumerable<T> items;
                            using (
                                var reader =
                                    await
                                        comm.ExecuteReaderAsync(cancellationToken)
                                            .ConfigureAwait(AsyncOrmConfig.ConfigureAwait))
                            {
                                items =
                                    await
                                        reader.ToGenericListAsync<T>(cancellationToken)
                                            .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                            }

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException("OperationCanceled", cancellationToken);

                            trans.Commit();
                            return items;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task ExecuteNonQueryAsync(string query, object dbParams = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (var comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType, query, commandTimeout, comm,
                                dbParams);
                            await
                                comm.ExecuteNonQueryAsync(cancellationToken)
                                    .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task ExecuteNonQueryAsync(IEnumerable<IBatchItem> batchItems,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        foreach (var setting in batchItems)
                        {
                            using (var comm = conn.CreateCommand())
                            {
                                SetupCommand(trans, CommandType, setting.CommandText, commandTimeout,
                                    comm, setting.DbParams);
                                await
                                    comm.ExecuteNonQueryAsync(cancellationToken)
                                        .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                            }
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, object dbParams = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object data;
                        using (var comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType, query, commandTimeout, comm,
                                dbParams);
                            data =
                                await
                                    comm.ExecuteScalarAsync(cancellationToken)
                                        .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                        return data;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, object dbParams = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int commandTimeout = 30,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (var trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object data;
                        using (var comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType, query, commandTimeout, comm,
                                dbParams);
                            data =
                                await
                                    comm.ExecuteScalarAsync(cancellationToken)
                                        .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                        try
                        {
                            return (T) Convert.ChangeType(data, typeof (T));
                        }
                        catch
                        {
                            return default(T);
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task BulkCopyAsync(IBulkCopySourceSetting bulkCopyBulkCopySourceSettings,
            IBulkCopyDestinationSetting bulkCopyDestinationSetting,
            SqlRowsCopiedEventHandler copiedEventHandler = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var sourceConnection = new SqlConnection(bulkCopyBulkCopySourceSettings.ConnectionString))
            {
                await sourceConnection.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                using (var commandSourceData = sourceConnection.CreateCommand())
                {
                    SetupCommand(CommandType, bulkCopyBulkCopySourceSettings.CommandText,
                        bulkCopyBulkCopySourceSettings.Timeout, commandSourceData,
                        bulkCopyBulkCopySourceSettings.Parameters);

                    var reader =
                        await
                            commandSourceData.ExecuteReaderAsync(cancellationToken)
                                .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                    using (var destinationConnection = new SqlConnection(bulkCopyDestinationSetting.ConnectionString))
                    {
                        await
                            destinationConnection.OpenAsync(cancellationToken)
                                .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                        using (var transaction = destinationConnection.BeginTransaction())
                        {
                            using (
                                var bulkCopy = new SqlBulkCopy(destinationConnection, SqlBulkCopyOptions.Default,
                                    transaction))
                            {
                                bulkCopy.DestinationTableName = bulkCopyDestinationSetting.DestinationTableName;
                                bulkCopy.BatchSize = bulkCopyDestinationSetting.BatchSize;
                                bulkCopy.BulkCopyTimeout = bulkCopyDestinationSetting.Timeout;
                                bulkCopy.NotifyAfter = bulkCopyDestinationSetting.NotifyAfter;
                                bulkCopy.EnableStreaming = true;
                                if (copiedEventHandler != null)
                                    bulkCopy.SqlRowsCopied += copiedEventHandler;
                                try
                                {
                                    await
                                        bulkCopy.WriteToServerAsync(reader, cancellationToken)
                                            .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                                    if (cancellationToken.IsCancellationRequested)
                                        throw new OperationCanceledException("OperationCanceled", cancellationToken);

                                    transaction.Commit();
                                }
                                catch
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                                finally
                                {
                                    reader.Close();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}

