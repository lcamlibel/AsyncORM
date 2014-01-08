using AsyncORM.MySql.interfaces;
using AsyncORM.MySql.Library;

namespace AsyncORM.MySql
{
    public class MySqlQueryAsyncFactory : IMySqlQueryAsyncFactory
    {
        private static MySqlQueryAsyncFactory _instance;
        public IMySqlQueryAsync Create(QueryAsyncType queryType)
        {
            return queryType == QueryAsyncType.DynamicQuery ? new MySqlDynamicQuery() as IMySqlQueryAsync : new MySqlStoredProcedure() as IMySqlQueryAsync;
        }
        public IMySqlQueryAsync Create(QueryAsyncType queryType,string connectionString)
        {
            return queryType == QueryAsyncType.DynamicQuery ? new MySqlDynamicQuery(connectionString) as IMySqlQueryAsync : new MySqlStoredProcedure(connectionString) as IMySqlQueryAsync;
        }
        public static MySqlQueryAsyncFactory Instance
        {
            get { return _instance ?? (_instance = new MySqlQueryAsyncFactory()); }
        }
    }
}
