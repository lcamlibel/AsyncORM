using System.Threading.Tasks;
using AsyncORM.DirectTable;

namespace AsyncORM.interfaces
{
    public interface ITable
    {
        Task<dynamic> InsertAsync(dynamic entity, TableSetting tableSetting);
        Task UpdateAsync(dynamic entity, TableSetting tableSetting);
        Task DeleteAsync(TableSetting tableSetting, dynamic entity);
    }
}
