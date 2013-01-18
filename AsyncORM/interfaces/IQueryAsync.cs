using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AsyncORM.interfaces
{
    public interface IQueryAsync
    {
        Task<IEnumerable<dynamic>> ExecuteAsync(string commandText,
                                                object dbParams = null,
                                                IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                int commandTimeout = 30);

        Task<IEnumerable<dynamic>> ExecuteMultipleResultSetAsync(string commandText,
                                                                 object dbParams = null,
                                                                 IsolationLevel isolationLevel =
                                                                     IsolationLevel.ReadCommitted,
                                                                 int commandTimeout = 30);

        Task ExecuteNonQueryAsync(string commandText,
                                  object dbParams = null, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                  int commandTimeout = 30);

        Task ExecuteNonQueryAsync(IEnumerable<IBatchSetting> batchSettings,
                                  IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                  int commandTimeout = 30);

        Task<object> ExecuteScalarAsync(string commandText,
                                        object dbParams = null,
                                        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                        int commandTimeout = 30);

        Task BulkCopyAsync(IBulkCopySourceSetting bulkCopyBulkCopySourceSettings,
                           IBulkCopyDestinationSetting bulkCopyDestinationSetting,
                           SqlRowsCopiedEventHandler copiedEventHandler = null);
    }
}