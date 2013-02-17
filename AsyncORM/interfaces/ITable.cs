using System.Threading.Tasks;
using AsyncORM.DirectTable;

namespace AsyncORM.interfaces
{
    public interface ITable
    {
        Task<dynamic> InsertAsync(TableOperationSetting tableOperationSetting, dynamic entity);
        Task UpdateAsync(TableOperationSetting tableOperationSetting, dynamic entity);
        Task DeleteAsync(TableOperationSetting tableOperationSetting, dynamic entity);
    }
}
