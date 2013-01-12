using AsyncORM.interfaces;

namespace AsyncORM
{
    public class BulkCopySourceSetting : IBulkCopySourceSetting
    {
        public string ConnectionString { get; set; }
        public string CommandText { get; set; }
        public object Parameters { get; set; }
        public int Timeout { get; set; }
    }
}