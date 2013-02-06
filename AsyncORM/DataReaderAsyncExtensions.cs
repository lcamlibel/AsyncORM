using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;

namespace AsyncORM
{
    public static class DataReaderExtensions
    {
        public static async Task<IEnumerable<dynamic>> ToExpandoListAsync(this SqlDataReader rdr)
        {
            return await Task.Run(async () =>
                                            {
                                                var result = new List<dynamic>();

                                                while (await rdr.ReadAsync())
                                                {
                                                    result.Add(await rdr.RecordToExpandoAsync());
                                                }
                                                return result;
                                            });
        }

        public static async Task<IEnumerable<IEnumerable<dynamic>>> ToExpandoMultipleListAsync(this SqlDataReader rdr)
        {
            return await Task.Run(async () =>
                                            {
                                                var result = new List<List<dynamic>>();
                                                do
                                                {
                                                    var list = new List<dynamic>();
                                                    while (await rdr.ReadAsync())
                                                    {
                                                        list.Add(await rdr.RecordToExpandoAsync());
                                                    }
                                                    if (list.Count > 0)
                                                        result.Add(list);
                                                } while (await rdr.NextResultAsync());

                                                return result;
                                            });
        }

        public static async Task<dynamic> RecordToExpandoAsync(this SqlDataReader rdr)
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
                                      });
        }
    }
}