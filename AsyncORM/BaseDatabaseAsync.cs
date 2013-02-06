using System.Collections.Generic;
using System.Data;
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
            await Task.Run(async () =>
                                     {
                                         comm.CommandType = commandType;
                                         comm.CommandText = commandText;
                                         comm.CommandTimeout = commandTimeout;
                                         comm.Transaction = transaction;
                                         if (parameters != null)
                                         {
                                             await AddParametersAsync(comm, parameters);
                                         }
                                     });
        }

        protected async Task SetupCommandAsync(CommandType commandType, string commandText,
                                               int commandTimeout,
                                               IDbCommand comm, object parameters = null)
        {
            await Task.Run(async () =>
                                     {
                                         comm.CommandType = commandType;
                                         comm.CommandText = commandText;
                                         comm.CommandTimeout = commandTimeout;
                                         if (parameters != null)
                                         {
                                             await AddParametersAsync(comm, parameters);
                                         }
                                     });
        }

        protected async Task AddParametersAsync(IDbCommand comm, object dbParams)
        {
            await Task.Run(async () =>
                                     {
                                         var dynoParams = dbParams as IDictionary<string, object>;
                                         if (dynoParams == null)
                                         {
                                             await ParseObject(comm, dbParams);
                                         }
                                         else
                                         {
                                             await ParseExpandaObject(comm, dynoParams);
                                         }
                                     });
        }

        private static async Task ParseObject(IDbCommand comm, object dbParams)
        {
            await Task.Run(() =>
                               {
                                   PropertyInfo[] props = dbParams.GetType().GetProperties();

                                   var length = props.Length;
                                   for (var index = 0; index < length; index++)
                                   {
                                       PropertyInfo item = props[index];
                                       IDbDataParameter param;
                                       if (item.PropertyType == typeof (IDbDataParameter))
                                       {
                                           param = (IDbDataParameter) item.GetValue(dbParams, null);
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
                               });
        }

        private static async Task ParseExpandaObject(IDbCommand comm, IEnumerable<KeyValuePair<string, object>> dbParams)
        {
            await Task.Run(() =>
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
                                           param = new SqlParameter
                                                       {
                                                           ParameterName = string.Concat("@", valuepair.Key),
                                                           Value = valuepair.Value,
                                                           Direction = ParameterDirection.Input
                                                       };
                                       }
                                       comm.Parameters.Add(param);
                                   }
                               });
        }
    }
}