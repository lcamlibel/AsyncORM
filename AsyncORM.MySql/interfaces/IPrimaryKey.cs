namespace AsyncORM.MySql.interfaces
{
    public interface IPrimaryKey
    {
        string Name { get; set; }
        bool IsIdentity { get; set; }
    }
}