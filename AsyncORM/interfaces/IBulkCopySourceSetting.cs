namespace AsyncORM.interfaces
{
    public interface IBulkCopySourceSetting
    {
        string ConnectionString { get; set; }
        string CommandText { get; set; }
        object Parameters { get; set; }
        int Timeout { get; set; }
    }
}