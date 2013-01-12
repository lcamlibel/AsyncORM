using AsyncORM.interfaces;

namespace AsyncORM
{
    public class BulkCopyDestinationSetting : IBulkCopyDestinationSetting
    {
        public string ConnectionString { get; set; }
        public string DestinationTableName { get; set; }
        public int Timeout { get; set; }
        public int BatchSize { get; set; }
        public int NotifyAfter { get; set; }
    }
}