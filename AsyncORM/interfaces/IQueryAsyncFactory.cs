using AsyncORM.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncORM.interfaces
{
    public interface IQueryAsyncFactory
    {
        IQueryAsync Create(QueryAsyncType queryType);
        IQueryAsync Create(QueryAsyncType queryType, string connectionString);
    }
}
