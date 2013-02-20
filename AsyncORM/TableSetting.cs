using System.Collections.Generic;
using AsyncORM.interfaces;

namespace AsyncORM
{
    public class TableSetting : ITableSetting
    {
        public List<IPrimaryKey> PrimaryKeys { get; set; }
        public string TableName { get; set; }
    }
}