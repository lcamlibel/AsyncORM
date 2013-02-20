using AsyncORM.interfaces;

namespace AsyncORM
{
    public class PrimaryKey : IPrimaryKey
    {
        public string Name { get; set; }
        public bool IsIdentity { get; set; }
    }
}