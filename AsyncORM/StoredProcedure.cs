using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.interfaces;

namespace AsyncORM
{
    public class StoredProcedure : BaseDatabaseAsync, IQueryAsync
    {
        public StoredProcedure(string connectionString)
            : base(connectionString)
        {
        }
        public StoredProcedure()
        {
            if (String.IsNullOrEmpty(Config.ConnectionString))
                throw new ArgumentNullException("please setup global connection String  use the following: StoredProcedure.Config.Connection");
        }
        #region IStoredProcedureAsync Members

        public async Task<IEnumerable<dynamic>> ExecuteAsync(string storedProcedure,
                                                             object dbParams = null,
                                                             IsolationLevel isolationLevel =
                                                                 IsolationLevel.ReadCommitted,
                                                             int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,cancellationToken,
                                                  dbParams);
                            IEnumerable<dynamic> items;
                            using (SqlDataReader reader = await comm.ExecuteReaderAsync(cancellationToken))
                            {
                                items = await reader.ToExpandoMultipleListAsync(cancellationToken);
                            }
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
        public async Task<IEnumerable<T>> ExecuteAsync<T>(string storedProcedure,
                                                           object dbParams = null,
                                                           IsolationLevel isolationLevel =
                                                               IsolationLevel.ReadCommitted,
                                                           int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
               await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,cancellationToken,
                                                  dbParams);
                            IEnumerable<T> items;
                            using (SqlDataReader reader = await comm.ExecuteReaderAsync(cancellationToken))
                            {
                                items = await reader.ToGenericListAsync<T>(cancellationToken,_localCache);
                            }
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
        public async Task ExecuteNonQueryAsync(string storedProcedure,
                                               object dbParams = null,
                                               IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                               int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
               await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,cancellationToken,
                                                  dbParams);
                            await comm.ExecuteNonQueryAsync(cancellationToken);
                        }
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
                                             int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
               await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        foreach (var setting in batchItems)
                        {
                            using (SqlCommand comm = conn.CreateCommand())
                            {
                                await
                                    SetupCommandAsync(trans, CommandType.StoredProcedure, setting.CommandText,
                                                      commandTimeout, comm, cancellationToken,setting.DbParams);
                                await comm.ExecuteNonQueryAsync(cancellationToken);
                            }
                        }
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
        public async Task<object> ExecuteScalarAsync(string storedProcedure,
                                                     object dbParams = null,
                                                     IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                     int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
               await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object data;
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,cancellationToken,
                                                  dbParams);
                            data = await comm.ExecuteScalarAsync(cancellationToken);
                        }
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
        public async Task<T> ExecuteScalarAsync<T>(string storedProcedure,
                                                    object dbParams = null,
                                                    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                    int commandTimeout = 30, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object data;
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,cancellationToken,
                                                  dbParams);
                            data = await comm.ExecuteScalarAsync(cancellationToken);
                        }
                        trans.Commit();
                        try
                        {
                            return (T)Convert.ChangeType(data, typeof(T));
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
                                        SqlRowsCopiedEventHandler copiedEventHandler = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var sourceConnection = new SqlConnection(bulkCopyBulkCopySourceSettings.ConnectionString))
            {
                await sourceConnection.OpenAsync(cancellationToken);

                using (SqlCommand commandSourceData = sourceConnection.CreateCommand())
                {
                    await
                        SetupCommandAsync(CommandType.StoredProcedure, bulkCopyBulkCopySourceSettings.CommandText,
                                          bulkCopyBulkCopySourceSettings.Timeout, commandSourceData, cancellationToken,bulkCopyBulkCopySourceSettings.Parameters);

                    SqlDataReader reader = await commandSourceData.ExecuteReaderAsync(cancellationToken);

                    using (var destinationConnection = new SqlConnection(bulkCopyDestinationSetting.ConnectionString))
                    {
                        await destinationConnection.OpenAsync(cancellationToken);

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
                                    await bulkCopy.WriteToServerAsync(reader, cancellationToken);
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