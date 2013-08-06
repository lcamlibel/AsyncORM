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
        protected readonly string ConnectionString;
        protected ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>> _localCache;
        public static AsyncOrmConfig Config = new AsyncOrmConfig();
        public BaseDatabaseAsync(string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString) {AsynchronousProcessing = true};
            ConnectionString = connBuilder.ConnectionString;
            _localCache = new ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>>();
        }
        public BaseDatabaseAsync()
        {
            var connBuilder = new SqlConnectionStringBuilder(Config.ConnectionString) { AsynchronousProcessing = true };
            ConnectionString = connBuilder.ConnectionString;
            _localCache = new ConcurrentDictionary<Type, Lazy<IEnumerable<PropertyInfo>>>();
        } 
        protected async Task SetupCommandAsync(IDbTransaction transaction, CommandType commandType, string commandText,
                                               int commandTimeout,
                                               IDbCommand comm, CancellationToken cancellationToken, object parameters = null)
        {
            await Task.Run(async () =>
                                     {
                                         comm.CommandType = commandType;
                                         comm.CommandText = commandText;
                                         comm.CommandTimeout = commandTimeout;
                                         comm.Transaction = transaction;
                                         if (parameters != null)
                                         {
                                             await AddParametersAsync(comm, parameters,cancellationToken);
                                         }
                                     },cancellationToken);
        }

        protected async Task SetupCommandAsync(CommandType commandType, string commandText,
                                               int commandTimeout,
                                               IDbCommand comm, CancellationToken cancellationToken ,object parameters = null)
        {
            await Task.Run(async () =>
                                     {
                                         comm.CommandType = commandType;
                                         comm.CommandText = commandText;
                                         comm.CommandTimeout = commandTimeout;
                                         if (parameters != null)
                                         {
                                             await AddParametersAsync(comm, parameters,cancellationToken);
                                         }
                                     },cancellationToken);
        }

        protected async Task AddParametersAsync(IDbCommand comm, object dbParams, CancellationToken cancellationToken )
        {
            await Task.Run(async () =>
                                     {
                                         var dynoParams = dbParams as IDictionary<string, object>;
                                         if (dynoParams == null)
                                         {
                                             await ParseObjectAsync(comm, dbParams,cancellationToken);
                                         }
                                         else
                                         {
                                             await ParseExpandaObjectAsync(comm, dynoParams,cancellationToken);
                                         }
                                     },cancellationToken);
        }

        private static async Task ParseObjectAsync(IDbCommand comm, object dbParams, CancellationToken cancellationToken )
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
                               },cancellationToken);
        }

        private static async Task ParseExpandaObjectAsync(IDbCommand comm, IEnumerable<KeyValuePair<string, object>> dbParams, CancellationToken cancellationToken )
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
                               },cancellationToken);
        }
    }
}