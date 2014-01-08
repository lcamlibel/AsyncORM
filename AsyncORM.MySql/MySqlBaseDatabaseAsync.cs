using System.Collections.Generic;
using System.Data;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace AsyncORM.MySql
{
    public abstract class MySqlBaseDatabaseAsync
    {
        protected readonly string ConnectionString;

        protected MySqlBaseDatabaseAsync(string connectionString)
        {
           // var connBuilder = new SqlConnectionStringBuilder(connectionString) {AsynchronousProcessing = true};
            ConnectionString = connectionString; //connBuilder.ConnectionString;
        }

        protected MySqlBaseDatabaseAsync()
        {
//            var connBuilder = new MysqlSqlConnectionStringBuilder(AsyncOrmConfig.ConnectionString)
//                                  {
//                                      AsynchronousProcessing
//                                          = true
//                                  };
            ConnectionString = MySqlAsyncOrmConfig.ConnectionString;// connBuilder.ConnectionString;
        }

        protected void SetupCommand(MySqlTransaction transaction, CommandType commandType, string commandText,
                                    int commandTimeout, MySqlCommand comm, object parameters = null)
        {
            comm.CommandType = commandType;
            comm.CommandText = commandText;
            comm.CommandTimeout = commandTimeout;
            comm.Transaction = transaction;
            if (parameters != null)
            {
                PrepareParameters(comm, parameters);
            }
        }

        protected void SetupCommand(CommandType commandType, string commandText, int commandTimeout, IDbCommand comm,
                                    object parameters = null)
        {
            comm.CommandType = commandType;
            comm.CommandText = commandText;
            comm.CommandTimeout = commandTimeout;
            if (parameters != null)
            {
                PrepareParameters(comm, parameters);
            }
        }

        private void PrepareParameters(IDbCommand comm, object dbParams)
        {
            var parameters = dbParams as IDictionary<string, object>;
            if (parameters == null)
            {
                ParseDBParamObject(comm, dbParams);
            }
            else
            {
                ParseExpandaDBParamObject(comm, parameters);
            }
        }

        private static void ParseDBParamObject(IDbCommand comm, object dbParams)
        {
            PropertyInfo[] props = dbParams.GetType().GetProperties();

            int length = props.Length;
            for (int index = 0; index < length; index++)
            {
                PropertyInfo item = props[index];
                IDataParameter param;
                if (item.PropertyType == typeof (IDataParameter))
                {
                    param = (IDataParameter) item.GetValue(dbParams, null);
                }
                else
                {
                    param = new MySqlParameter
                                {
                                    ParameterName =string.Concat("@", item.Name),
                                    Value = item.GetValue(dbParams, null),
                                    Direction = ParameterDirection.Input
                                };
                }
                if (param != null)
                    comm.Parameters.Add(param);
            }
        }

        private static void ParseExpandaDBParamObject(IDbCommand comm,
                                                      IEnumerable<KeyValuePair<string, object>> dbParams)
        {
            foreach (var valuepair in dbParams)
            {
                IDataParameter param;
                if (valuepair.Value is IDataParameter)
                {
                    param = valuepair.Value as IDataParameter;
                }
                else
                {
                    param = new MySqlParameter
                                {
                                    ParameterName =  string.Concat("@", valuepair.Key),
                                    Value = valuepair.Value,
                                    Direction = ParameterDirection.Input
                                };
                }
                comm.Parameters.Add(param);
            }
        }
    }
}