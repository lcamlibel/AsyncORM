using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM
{
    public class BaseDatabaseAsync
    {
        internal static ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>> ParameterCache =
            new ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>>();

        protected readonly string ConnectionString;

        public BaseDatabaseAsync(string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString) {AsynchronousProcessing = true};
            ConnectionString = connBuilder.ConnectionString;
        }

        public BaseDatabaseAsync()
        {
            var connBuilder = new SqlConnectionStringBuilder(AsyncOrmConfig.ConnectionString)
                                  {
                                      AsynchronousProcessing
                                          = true
                                  };
            ConnectionString = connBuilder.ConnectionString;
        }

        protected async Task SetupCommandAsync(IDbTransaction transaction, CommandType commandType, string commandText,
                                               int commandTimeout, IDbCommand comm, CancellationToken cancellationToken,
                                               object parameters = null)
        {
            await Task.Run(() =>
                               {
                                   comm.CommandType = commandType;
                                   comm.CommandText = commandText;
                                   comm.CommandTimeout = commandTimeout;
                                   comm.Transaction = transaction;
                                   if (parameters != null)
                                   {
                                       PrepareParameters(comm, parameters);
                                   }
                               }, cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
        }

        protected async Task SetupCommandAsync(CommandType commandType, string commandText, int commandTimeout,
                                               IDbCommand comm, CancellationToken cancellationToken,
                                               object parameters = null)
        {
            await Task.Run(() =>
                               {
                                   comm.CommandType = commandType;
                                   comm.CommandText = commandText;
                                   comm.CommandTimeout = commandTimeout;
                                   if (parameters != null)
                                   {
                                       PrepareParameters(comm, parameters);
                                   }
                               }, cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
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
                    param = new SqlParameter
                                {
                                    ParameterName = string.Concat("@", item.Name),
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
                    param = new SqlParameter
                                {
                                    ParameterName = string.Concat("@", valuepair.Key),
                                    Value = valuepair.Value,
                                    Direction = ParameterDirection.Input
                                };
                }
                comm.Parameters.Add(param);
            }
        }
    }
}