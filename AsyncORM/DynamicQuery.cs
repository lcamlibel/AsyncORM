using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.Extensions;
using AsyncORM.interfaces;

namespace AsyncORM
{
    public class DynamicQuery : BaseDatabaseAsync, IQueryAsync
    {
        public DynamicQuery(string connectionString) : base(connectionString)
        {
        }

        public DynamicQuery()
        {
            if (String.IsNullOrEmpty(AsyncOrmConfig.ConnectionString))
                throw new ArgumentNullException(
                    "please setup global connectionstring  use the following: AsyncOrmConfig.ConnectionString");
        }

        #region IQueryAsync Members

        public async Task<IEnumerable<dynamic>> ExecuteAsync(string commandText, object dbParams = null,
                                                             IsolationLevel isolationLevel =
                                                                 IsolationLevel.ReadCommitted, int commandTimeout = 30,
                                                             CancellationToken cancellationToken =
                                                                 default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);
                            IEnumerable<dynamic> items;
                            using (
                                SqlDataReader reader =
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

        public async Task<IEnumerable<T>> ExecuteAsync<T>(string commandText, object dbParams = null,
                                                          IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                          int commandTimeout = 30,
                                                          CancellationToken cancellationToken =
                                                              default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);
                            IEnumerable<T> items;
                            using (
                                SqlDataReader reader =
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

        public async Task ExecuteNonQueryAsync(string commandText, object dbParams = null,
                                               IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                               int commandTimeout = 30,
                                               CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

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
                                               int commandTimeout = 259200,
                                               CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        foreach (IBatchItem setting in batchItems)
                        {
                            using (SqlCommand comm = conn.CreateCommand())
                            {
                                SetupCommand(trans, CommandType.Text, setting.CommandText, commandTimeout, comm,
                                             setting.DbParams);
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

        public async Task<object> ExecuteScalarAsync(string commandText, object dbParams = null,
                                                     IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                     int commandTimeout = 30,
                                                     CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object result;
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

                            result =
                                await
                                comm.ExecuteScalarAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                        return result;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string commandText, object dbParams = null,
                                                   IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                   int commandTimeout = 30,
                                                   CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object result;
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

                            result =
                                await
                                comm.ExecuteScalarAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                        }
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException("OperationCanceled", cancellationToken);

                        trans.Commit();
                        try
                        {
                            return (T) Convert.ChangeType(result, typeof (T));
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

                using (SqlCommand commandSourceData = sourceConnection.CreateCommand())
                {
                    SetupCommand(CommandType.Text, bulkCopyBulkCopySourceSettings.CommandText,
                                 bulkCopyBulkCopySourceSettings.Timeout, commandSourceData,
                                 bulkCopyBulkCopySourceSettings.Parameters);

                    SqlDataReader reader =
                        await
                        commandSourceData.ExecuteReaderAsync(cancellationToken)
                                         .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                    using (var destinationConnection = new SqlConnection(bulkCopyDestinationSetting.ConnectionString))
                    {
                        await
                            destinationConnection.OpenAsync(cancellationToken)
                                                 .ConfigureAwait(AsyncOrmConfig.ConfigureAwait);

                        using (SqlTransaction transaction = destinationConnection.BeginTransaction())
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