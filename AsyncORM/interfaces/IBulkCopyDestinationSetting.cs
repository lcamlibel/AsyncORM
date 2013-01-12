namespace AsyncORM.interfaces
{
    public interface IBulkCopyDestinationSetting
    {
        string ConnectionString { get; set; }
        string DestinationTableName { get; set; }
        int Timeout { get; set; }
        int BatchSize { get; set; }
        int NotifyAfter { get; set; }
    }
}