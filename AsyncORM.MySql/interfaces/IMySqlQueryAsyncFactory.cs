using AsyncORM.MySql.Library;

namespace AsyncORM.MySql.interfaces
{
    public interface IMySqlQueryAsyncFactory
    {
        IMySqlQueryAsync Create(QueryAsyncType queryType);
        IMySqlQueryAsync Create(QueryAsyncType queryType, string connectionString);
    }
}
