using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM.Extensions
{
    internal static class DataReaderExtensions
    {
        
        internal static async Task<IEnumerable<T>> ToGenericListAsync<T>(this SqlDataReader dr,
                                                                         CancellationToken cancellationToken)
        {
            var generatedGenericObjects = new List<T>();
            var sampleInstance = Activator.CreateInstance<T>();
            Type instanceType = sampleInstance.GetType();

            while (await dr.ReadAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait))
            {
                var instance = Activator.CreateInstance<T>();
                IEnumerable<PropertyInfo> properties;
                if (AsyncOrmConfig.EnableParameterCache)
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
                    properties.Where(prop => !Equals(dr[prop.Name], DBNull.Value)))
                {
                    Action<object, object> setAccessor = ReflectionHelper.BuildSetAccessor(prop.GetSetMethod());
                    setAccessor(instance, dr[prop.Name]);
                   // prop.SetValue(instance, dr[prop.Name], null);
                }
                generatedGenericObjects.Add(instance);
            }
            return generatedGenericObjects;
        }

        internal static async Task<IEnumerable<IEnumerable<dynamic>>> ToExpandoMultipleListAsync(this SqlDataReader rdr,
                                                                                                 CancellationToken
                                                                                                     cancellationToken)
        {
            var result = new List<List<dynamic>>();
            do
            {
                var generatedDynamicObjects = new List<dynamic>();
                while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait))
                {
                    generatedDynamicObjects.Add(rdr.RecordToExpando());
                }
                if (generatedDynamicObjects.Count > 0)
                    result.Add(generatedDynamicObjects);
            } while (await rdr.NextResultAsync(cancellationToken).ConfigureAwait(AsyncOrmConfig.ConfigureAwait));

            return result;
        }

        private static dynamic RecordToExpando(this SqlDataReader rdr)
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