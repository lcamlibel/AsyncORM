#region

using System;
using System.Data;
using AsyncORM.interfaces;

#endregion

namespace AsyncORM
{
    public class DynamicQuery : QueryAsync, IDynamicQuery
    {
        public DynamicQuery(string connectionString) : base(connectionString)
        {
            CommandType = CommandType.Text;
        }

        public DynamicQuery()
        {
            CommandType = CommandType.Text;
            if (String.IsNullOrEmpty(AsyncOrmConfig.ConnectionString))
                throw new ArgumentNullException(
                    "please setup global connectionstring use the following: AsyncOrmConfig.ConnectionString");
        }
    }
}