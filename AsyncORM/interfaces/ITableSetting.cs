using System.Collections.Generic;

namespace AsyncORM.interfaces
{
    public interface ITableSetting
    {
        List<IPrimaryKey> PrimaryKeys { get; set; }
        string TableName { get; set; }
    }
}