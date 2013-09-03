using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM.interfaces
{
    public interface IQueryAsync
    {
        Task<IEnumerable<dynamic>> ExecuteAsync(string commandText, object dbParams = null,
                                                IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                int commandTimeout = 30,
                                                CancellationToken cancellationToken = default(CancellationToken));


        Task<IEnumerable<T>> ExecuteAsync<T>(string commandText, object dbParams = null,
                                             IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                             int commandTimeout = 30,
                                             CancellationToken cancellationToken = default(CancellationToken));


        Task ExecuteNonQueryAsync(string commandText, object dbParams = null,
                                  IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int commandTimeout = 30,
                                  CancellationToken cancellationToken = default(CancellationToken));

        Task ExecuteNonQueryAsync(IEnumerable<IBatchItem> batchItems,
                                  IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int commandTimeout = 30,
                                  CancellationToken cancellationToken = default(CancellationToken));

        Task<object> ExecuteScalarAsync(string commandText, object dbParams = null,
                                        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                        int commandTimeout = 30,
                                        CancellationToken cancellationToken = default(CancellationToken));

        Task<T> ExecuteScalarAsync<T>(string commandText, object dbParams = null,
                                      IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                      int commandTimeout = 30,
                                      CancellationToken cancellationToken = default(CancellationToken));

        Task BulkCopyAsync(IBulkCopySourceSetting bulkCopyBulkCopySourceSettings,
                           IBulkCopyDestinationSetting bulkCopyDestinationSetting,
                           SqlRowsCopiedEventHandler copiedEventHandler = null,
                           CancellationToken cancellationToken = default(CancellationToken));
    }
}