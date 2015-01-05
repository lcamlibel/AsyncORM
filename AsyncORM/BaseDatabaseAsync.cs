using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace AsyncORM
{
    public abstract class BaseDatabaseAsync
    {
        protected readonly string ConnectionString;
        protected CommandType CommandType;
        protected BaseDatabaseAsync(string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString) { AsynchronousProcessing = true };
            ConnectionString = connBuilder.ConnectionString;
        }

        protected BaseDatabaseAsync()
        {
            var connBuilder = new SqlConnectionStringBuilder(AsyncOrmConfig.ConnectionString)
            {
                AsynchronousProcessing
                    = true
            };
            ConnectionString = connBuilder.ConnectionString;
        }

        protected void SetupCommand(IDbTransaction transaction, CommandType commandType, string commandText,
                                    int commandTimeout, IDbCommand comm, object parameters = null)
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
                if (item.PropertyType == typeof(IDataParameter))
                {
                    param = (IDataParameter)item.GetValue(dbParams, null);
                }
                else
                {
                    param = new SqlParameter
                    {
                        ParameterName = string.Concat("@", item.Name),
                        Value = item.GetValue(dbParams, null) ?? DBNull.Value,
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
                    param = new SqlParameter
                    {
                        ParameterName = string.Concat("@", valuepair.Key),
                        Value = valuepair.Value ?? DBNull.Value,
                        Direction = ParameterDirection.Input
                    };
                }
                comm.Parameters.Add(param);
            }
        }
    }
}