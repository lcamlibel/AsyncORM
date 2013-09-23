using AsyncORM.interfaces;
using AsyncORM.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncORM
{
    public class QueryAsyncFactory : IQueryAsyncFactory
    {
        private static QueryAsyncFactory _instance;
        public IQueryAsync Create(QueryAsyncType queryType)
        {
            return queryType == QueryAsyncType.DynamicQuery ? new DynamicQuery() as IQueryAsync : new StoredProcedure() as IQueryAsync;
        }
        public IQueryAsync Create(QueryAsyncType queryType,string connectionString)
        {
            return queryType == QueryAsyncType.DynamicQuery ? new DynamicQuery(connectionString) as IQueryAsync : new StoredProcedure(connectionString) as IQueryAsync;
        }
        public static QueryAsyncFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new QueryAsyncFactory();
                return _instance;
            }
        }
    }
}
