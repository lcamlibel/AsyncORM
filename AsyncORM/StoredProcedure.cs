using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        #region IStoredProcedureAsync Members

        public async Task<IEnumerable<dynamic>> ExecuteAsync(string storedProcedure,
                                                             object dbParams = null,
                                                             IsolationLevel isolationLevel =
                                                                 IsolationLevel.ReadCommitted,
                                                             int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,
                                                  dbParams);
                            IEnumerable<dynamic> items;
                            using (SqlDataReader reader = await comm.ExecuteReaderAsync())
                            {
                                items = await reader.ToExpandoMultipleListAsync();
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

        public async Task ExecuteNonQueryAsync(string storedProcedure,
                                               object dbParams = null,
                                               IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                               int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,
                                                  dbParams);
                            await comm.ExecuteNonQueryAsync();
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
        public async Task ExecuteNonQueryAsync(IEnumerable<IBatchSetting> batchSettings,
                                             IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                             int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        foreach (var setting in batchSettings)
                        {
                            using (SqlCommand comm = conn.CreateCommand())
                            {
                                await
                                    SetupCommandAsync(trans, CommandType.StoredProcedure, setting.CommandText,
                                                      commandTimeout, comm, setting.DbParams);
                                await comm.ExecuteNonQueryAsync();
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
                                                     int commandTimeout = 30)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object data;
                        using (SqlCommand comm = conn.CreateCommand())
                        {
                            await
                                SetupCommandAsync(trans, CommandType.StoredProcedure, storedProcedure, commandTimeout,
                                                  comm,
                                                  dbParams);
                            data = await comm.ExecuteScalarAsync();
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

        public async Task BulkCopyAsync(IBulkCopySourceSetting bulkCopyBulkCopySourceSettings,
                                        IBulkCopyDestinationSetting bulkCopyDestinationSetting,
                                        SqlRowsCopiedEventHandler copiedEventHandler = null)
        {
            using (var sourceConnection = new SqlConnection(bulkCopyBulkCopySourceSettings.ConnectionString))
            {
                await sourceConnection.OpenAsync();

                using (SqlCommand commandSourceData = sourceConnection.CreateCommand())
                {
                    await
                        SetupCommandAsync(CommandType.StoredProcedure, bulkCopyBulkCopySourceSettings.CommandText,
                                          bulkCopyBulkCopySourceSettings.Timeout, commandSourceData,
                                          bulkCopyBulkCopySourceSettings.Parameters);

                    SqlDataReader reader = await commandSourceData.ExecuteReaderAsync();

                    using (var destinationConnection = new SqlConnection(bulkCopyDestinationSetting.ConnectionString))
                    {
                        await destinationConnection.OpenAsync();

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
                                    await bulkCopy.WriteToServerAsync(reader);
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