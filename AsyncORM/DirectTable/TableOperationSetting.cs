using System.Collections.Generic;
using AsyncORM.interfaces;

namespace AsyncORM.DirectTable
{
    public class TableOperationSetting : ITableOperationSetting
    {
        public List<IPrimaryKey> PrimaryKeys { get; set; }
        public string TableName { get; set; }
        public string Where { get; set; }
      
    }
}