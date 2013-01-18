namespace AsyncORM.interfaces
{
    public interface IBatchSetting
    {
        string CommandText { get; set; }
        object DbParams { get; set; }
    }
}