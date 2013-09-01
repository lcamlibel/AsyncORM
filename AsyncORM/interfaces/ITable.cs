using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM.interfaces
{
    public interface ITable
    {
        Task<dynamic> InsertAsync(dynamic entity, TableSetting tableSetting,
                                  CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateAsync(dynamic entity, TableSetting tableSetting, string where = null,
                         CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteAsync(TableSetting tableSetting, dynamic entity, string where = null,
                         CancellationToken cancellationToken = default(CancellationToken));
    }
}