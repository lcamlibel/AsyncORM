namespace AsyncORM.MySql.interfaces
{
    public interface IBatchItem
    {
        string CommandText { get; set; }
        object DbParams { get; set; }
    }
}