using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.MySql.Library;
using MySql.Data.MySqlClient;

namespace AsyncORM.MySql.Extensions
{
    internal static class DataReaderExtensions
    {
        internal static async Task<IEnumerable<T>> ToGenericListAsync<T>(this DbDataReader dr,
                                                                         CancellationToken cancellationToken)
        {
            var generatedGenericObjects = new List<T>();
            var sampleInstance = Activator.CreateInstance<T>();
            Type instanceType = sampleInstance.GetType();
            var columnNames = dr.Columns();
            while (await dr.ReadAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait))
            {
                var instance = Activator.CreateInstance<T>();
                IEnumerable<PropertyInfo> properties;
                if (MySqlAsyncOrmConfig.EnableParameterCache)
                {
                    properties =
                        CacheManager.ParameterCache.GetOrAdd(instanceType,
                                                             new Lazy<IEnumerable<PropertyInfo>>(
                                                                 () => instance.GetType().GetProperties())).Value;
                }
                else
                {
                    properties = instance.GetType().GetProperties();
                }
                foreach (PropertyInfo prop in
                    properties.Where(prop => columnNames.Any(x=>x.Equals(prop.Name,StringComparison.InvariantCultureIgnoreCase)) && !Equals(dr[prop.Name], DBNull.Value)))
                {
                    prop.SetValue(instance, dr[prop.Name]);
                  //  Action<object, object> setAccessor = ReflectionHelper.BuildSetAccessor(prop.GetSetMethod());
                   // setAccessor(instance, dr[prop.Name]);
                   
                }
                generatedGenericObjects.Add(instance);
            }
            return generatedGenericObjects;
        }
        public static IEnumerable<string> Columns(this DbDataReader dr)
        {
            var columnNames = new List<string>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
               columnNames.Add(dr.GetName(i));
                   
            }
            return columnNames;
        }
        public static bool HasColumn(this MySqlDataReader dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        internal static async Task<IEnumerable<IEnumerable<dynamic>>> ToExpandoMultipleListAsync(this DbDataReader rdr,
                                                                                                 CancellationToken
                                                                                                     cancellationToken)
        {
            var result = new List<List<dynamic>>();
            do
            {
                var generatedDynamicObjects = new List<dynamic>();
                while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait))
                {
                    generatedDynamicObjects.Add(rdr.RecordToExpando());
                }
                if (generatedDynamicObjects.Count > 0)
                    result.Add(generatedDynamicObjects);
            } while (await rdr.NextResultAsync(cancellationToken).ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait));

            return result;
        }

        private static dynamic RecordToExpando(this DbDataReader rdr)
        {
            dynamic expandoObject = new ExpandoObject();
            var dataRow = expandoObject as IDictionary<string, object>;
            int fieldCount = rdr.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                object dataItem = rdr[i];
                dataRow.Add(rdr.GetName(i), DBNull.Value.Equals(dataItem) ? null : dataItem);
            }
            return expandoObject;
        }
    }
}