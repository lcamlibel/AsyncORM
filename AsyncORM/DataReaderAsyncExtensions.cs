using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM
{
    internal static class DataReaderExtensions
    {
        internal static async Task<IEnumerable<dynamic>> ToExpandoListAsync(this SqlDataReader rdr,
                                                                          CancellationToken cancellationToken)
        {
            return await new AsyncLazy<IEnumerable<dynamic>>(async () =>
                                                                       {
                                                                           var result = new List<dynamic>();

                                                                           while (
                                                                               await
                                                                               rdr.ReadAsync(cancellationToken)
                                                                                  .ConfigureAwait(
                                                                                      AsyncOrmConfig.ConfigureAwait))
                                                                           {
                                                                               result.Add(
                                                                                   await
                                                                                   rdr.RecordToExpandoAsync(
                                                                                       cancellationToken)
                                                                                      .ConfigureAwait(
                                                                                          AsyncOrmConfig.ConfigureAwait));
                                                                           }
                                                                           return result;
                                                                       }, cancellationToken);
        }

        internal static async Task<IEnumerable<T>> ToGenericListAsync<T>(this SqlDataReader dr,
                                                                       CancellationToken cancellationToken,
                                                                       ConcurrentDictionary
                                                                           <Type, Lazy<IEnumerable<PropertyInfo>>>
                                                                           localCache)
        {
            return await new AsyncLazy<IEnumerable<T>>(async () =>
                                                                 {
                                                                     var list = new List<T>();
                                                                     var sampleInstance = Activator.CreateInstance<T>();
                                                                     Type instanceType = sampleInstance.GetType();

                                                                     while (
                                                                         await
                                                                         dr.ReadAsync(cancellationToken)
                                                                           .ConfigureAwait(AsyncOrmConfig.ConfigureAwait))
                                                                     {
                                                                         var instance = Activator.CreateInstance<T>();
                                                                         IEnumerable<PropertyInfo> properties;
                                                                         if (AsyncOrmConfig.EnableParameterCache)
                                                                         {
                                                                             properties =
                                                                                 localCache.GetOrAdd(instanceType,
                                                                                                     new Lazy
                                                                                                         <
                                                                                                         IEnumerable
                                                                                                         <PropertyInfo>>
                                                                                                         (() =>
                                                                                                          instance
                                                                                                              .GetType()
                                                                                                              .GetProperties
                                                                                                              ())).Value;
                                                                         }
                                                                         else
                                                                         {
                                                                             properties =
                                                                                 instance.GetType().GetProperties();
                                                                         }
                                                                         foreach (PropertyInfo prop in
                                                                             properties.Where(
                                                                                 prop =>
                                                                                 !Equals(dr[prop.Name], DBNull.Value)))
                                                                         {
                                                                             prop.SetValue(instance, dr[prop.Name], null);
                                                                         }
                                                                         list.Add(instance);
                                                                     }
                                                                     return list;
                                                                 }, cancellationToken);
        }

        internal static async Task<IEnumerable<IEnumerable<dynamic>>> ToExpandoMultipleListAsync(this SqlDataReader rdr,
                                                                                               CancellationToken
                                                                                                   cancellationToken)
        {
            return await new AsyncLazy<IEnumerable<IEnumerable<dynamic>>>(async () =>
                                                                                    {
                                                                                        var result =
                                                                                            new List<List<dynamic>>();
                                                                                        do
                                                                                        {
                                                                                            var list =
                                                                                                new List<dynamic>();
                                                                                            while (
                                                                                                await
                                                                                                rdr.ReadAsync(
                                                                                                    cancellationToken)
                                                                                                   .ConfigureAwait(
                                                                                                       AsyncOrmConfig
                                                                                                           .ConfigureAwait))
                                                                                            {
                                                                                                list.Add(
                                                                                                    await
                                                                                                    rdr
                                                                                                        .RecordToExpandoAsync
                                                                                                        (cancellationToken)
                                                                                                        .ConfigureAwait(
                                                                                                            AsyncOrmConfig
                                                                                                                .ConfigureAwait));
                                                                                            }
                                                                                            if (list.Count > 0)
                                                                                                result.Add(list);
                                                                                        } while (
                                                                                            await
                                                                                            rdr.NextResultAsync(
                                                                                                cancellationToken)
                                                                                               .ConfigureAwait(
                                                                                                   AsyncOrmConfig
                                                                                                       .ConfigureAwait));

                                                                                        return result;
                                                                                    }, cancellationToken);
        }

        internal static async Task<dynamic> RecordToExpandoAsync(this SqlDataReader rdr,
                                                               CancellationToken cancellationToken)
        {
            return await new AsyncLazy<dynamic>(() =>
                                                    {
                                                        dynamic e = new ExpandoObject();
                                                        var dataRow = e as IDictionary<string, object>;
                                                        int fieldCount = rdr.FieldCount;
                                                        for (int i = 0; i < fieldCount; i++)
                                                        {
                                                            object dataItem = rdr[i];
                                                            dataRow.Add(rdr.GetName(i),
                                                                        DBNull.Value.Equals(dataItem) ? null : dataItem);
                                                        }
                                                        return e;
                                                    }, cancellationToken);
        }
    }
}