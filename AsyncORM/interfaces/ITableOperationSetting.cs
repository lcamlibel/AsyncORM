using System.Collections.Generic;
using AsyncORM.DirectTable;

namespace AsyncORM.interfaces
{
    public interface ITableOperationSetting
    {
        List<IPrimaryKey> PrimaryKeys { get; set; }
        string TableName { get; set; }
        string Where { get; set; }
        

    }
}