using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.MySql.Extensions;
using AsyncORM.MySql.interfaces;
using MySql.Data.MySqlClient;

namespace AsyncORM.MySql
{
    public class MySqlDynamicQuery : MySqlBaseDatabaseAsync, IMySqlDynamicQuery
    {
        public MySqlDynamicQuery(string connectionString) : base(connectionString)
        {
        }

        public MySqlDynamicQuery()
        {
            if (String.IsNullOrEmpty(MySqlAsyncOrmConfig.ConnectionString))
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (MySqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);
                            IEnumerable<dynamic> items;
                            using (
                                DbDataReader reader =
                                    await
                                        comm.ExecuteReaderAsync(cancellationToken)
                                            .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait))
                            {
                                items =
                                    await
                                        reader.ToExpandoMultipleListAsync(cancellationToken)
                                            .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (MySqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);
                            IEnumerable<T> items;
                            using (
                                DbDataReader reader =
                                    await
                                        comm.ExecuteReaderAsync(cancellationToken)
                                            .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait))
                            {
                                items =
                                    await
                                        reader.ToGenericListAsync<T>(cancellationToken)
                                            .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        using (MySqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

                            await
                                comm.ExecuteNonQueryAsync(cancellationToken)
                                    .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        foreach (IBatchItem setting in batchItems)
                        {
                            using (MySqlCommand comm = conn.CreateCommand())
                            {
                                SetupCommand(trans, CommandType.Text, setting.CommandText, commandTimeout, comm,
                                    setting.DbParams);
                                await
                                    comm.ExecuteNonQueryAsync(cancellationToken)
                                        .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object result;
                        using (MySqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

                            result =
                                await
                                    comm.ExecuteScalarAsync(cancellationToken)
                                        .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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
            using (var conn = new MySqlConnection(ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                using (MySqlTransaction trans = conn.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        object result;
                        using (MySqlCommand comm = conn.CreateCommand())
                        {
                            SetupCommand(trans, CommandType.Text, commandText, commandTimeout, comm, dbParams);

                            result =
                                await
                                    comm.ExecuteScalarAsync(cancellationToken)
                                        .ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
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

        #endregion
    }
}