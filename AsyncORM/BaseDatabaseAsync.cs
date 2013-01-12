using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace AsyncORM
{
    public class BaseDatabaseAsync
    {
        protected readonly string ConnectionString;

        public BaseDatabaseAsync(string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString) {AsynchronousProcessing = true};
            ConnectionString = connBuilder.ConnectionString;
        }

        protected async Task SetupCommandAsync(IDbTransaction transaction, CommandType commandType, string commandText,
                                               int commandTimeout,
                                               IDbCommand comm, object parameters = null)
        {
            comm.CommandType = commandType;
            comm.CommandText = commandText;
            comm.CommandTimeout = commandTimeout;
            comm.Transaction = transaction;
            if (parameters != null)
            {
                await AddParametersAsync(comm, parameters);
            }
        }

        protected async Task SetupCommandAsync(CommandType commandType, string commandText,
                                               int commandTimeout,
                                               IDbCommand comm, object parameters = null)
        {
            comm.CommandType = commandType;
            comm.CommandText = commandText;
            comm.CommandTimeout = commandTimeout;
            if (parameters != null)
            {
                await AddParametersAsync(comm, parameters);
            }
        }

        protected async Task AddParametersAsync(IDbCommand comm, object dbParams)
        {
            await Task.Run(() =>
                               {
                                   var dynoParams = dbParams as IDictionary<string, object>;
                                   if (dynoParams == null)
                                   {
                                       ParseObject(comm, dbParams);
                                   }
                                   else
                                   {
                                       ParseExpandaObject(comm, dynoParams);
                                   }
                               });
        }

        private static void ParseObject(IDbCommand comm, object dbParams)
        {
            PropertyInfo[] props = dbParams.GetType().GetProperties();
            for (int index = 0; index < props.Length; index++)
            {
                IDbDataParameter param;
                PropertyInfo item = props[index];
                if (item.PropertyType == typeof (IDbDataParameter) || item.PropertyType == typeof (DbParameter) ||
                    item.PropertyType.IsSubclassOf(typeof (DbParameter)))
                {
                    param = (IDbDataParameter) item.GetValue(dbParams, null);
                }
                else
                {
                    param = comm.CreateParameter();
                    param.ParameterName = string.Concat("@", item.Name);
                    param.Value = item.GetValue(dbParams, null);
                }
                if (param != null)
                    comm.Parameters.Add(param);
            }
        }

        private static void ParseExpandaObject(IDbCommand comm, IEnumerable<KeyValuePair<string, object>> dbParams)
        {
            foreach (var valuepair in dbParams)
            {
                IDbDataParameter param;
                var dataParameter = valuepair.Value as IDbDataParameter;
                if (dataParameter != null)
                {
                    param = dataParameter;
                }
                else
                {
                    param = comm.CreateParameter();
                    param.ParameterName = string.Concat("@", valuepair.Key);
                    param.Value = valuepair.Value;
                }
                comm.Parameters.Add(param);
            }
        }
    }
}