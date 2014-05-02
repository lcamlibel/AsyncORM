#region

using System;
using System.Data;
using AsyncORM.interfaces;

#endregion

namespace AsyncORM
{
    public class StoredProcedure : QueryAsync, IStoredProcedure
    {
        
        public StoredProcedure(string connectionString) : base(connectionString)
        {
            CommandType = CommandType.StoredProcedure;
        }

        public StoredProcedure()
        {
            CommandType = CommandType.StoredProcedure;
            if (String.IsNullOrEmpty(AsyncOrmConfig.ConnectionString))
                throw new ArgumentNullException(
                    "please setup global connectionstring use the following: AsyncOrmConfig.ConnectionString");
        }
    }
}