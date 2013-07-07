using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM
{
    public static class DataReaderExtensions
    {
        public static async Task<IEnumerable<dynamic>> ToExpandoListAsync(this SqlDataReader rdr, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
                                            {
                                                var result = new List<dynamic>();

                                                while (await rdr.ReadAsync(cancellationToken))
                                                {
                                                    result.Add(await rdr.RecordToExpandoAsync(cancellationToken));
                                                }
                                                return result;
                                            },cancellationToken);
        }

        public static async Task<IEnumerable<T>> ToGenericList<T>(this SqlDataReader dr, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
                                            {
                                                var list = new List<T>();
                                                while (await dr.ReadAsync(cancellationToken))
                                                {
                                                    var instance = Activator.CreateInstance<T>();
                                                    var properties = instance.GetType().GetProperties();
                                                    foreach (PropertyInfo prop in properties.Where(prop => !Equals(dr[prop.Name], DBNull.Value)))
                                                    {
                                                        prop.SetValue(instance, dr[prop.Name], null);
                                                    }
                                                    list.Add(instance);
                                                }
                                                return list;
                                            },cancellationToken);
        }

        public static async Task<IEnumerable<IEnumerable<dynamic>>> ToExpandoMultipleListAsync(this SqlDataReader rdr, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
                                            {
                                                var result = new List<List<dynamic>>();
                                                do
                                                {
                                                    var list = new List<dynamic>();
                                                    while (await rdr.ReadAsync(cancellationToken))
                                                    {
                                                        list.Add(await rdr.RecordToExpandoAsync(cancellationToken));
                                                    }
                                                    if (list.Count > 0)
                                                        result.Add(list);
                                                } while (await rdr.NextResultAsync(cancellationToken));

                                                return result;
                                            },cancellationToken);
        }

        public static async Task<dynamic> RecordToExpandoAsync(this SqlDataReader rdr, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
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
                                      },cancellationToken);
        }
    }
}