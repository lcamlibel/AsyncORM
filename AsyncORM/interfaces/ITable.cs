using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM.interfaces
{
    public interface ITable
    {
        Task<dynamic> InsertAsync(dynamic entity, TableSetting tableSetting);

        Task UpdateAsync(dynamic entity, TableSetting tableSetting, string where = null);

        Task DeleteAsync(dynamic entity, TableSetting tableSetting, string where = null);
    }
}