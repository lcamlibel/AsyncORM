#region

using AsyncORM.interfaces;
using AsyncORM.Library;

#endregion

namespace AsyncORM
{
    public class QueryAsyncFactory : IQueryAsyncFactory
    {
        private static QueryAsyncFactory _instance;

        public static QueryAsyncFactory Instance
        {
            get { return _instance ?? (_instance = new QueryAsyncFactory()); }
        }

        public IQueryAsync Create(QueryAsyncType queryType)
        {
            return queryType == QueryAsyncType.DynamicQuery ? new DynamicQuery() : new StoredProcedure() as IQueryAsync;
        }

        public IQueryAsync Create(QueryAsyncType queryType, string connectionString)
        {
            return queryType == QueryAsyncType.DynamicQuery
                ? new DynamicQuery(connectionString)
                : new StoredProcedure(connectionString) as IQueryAsync;
        }
    }
}