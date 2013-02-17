using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AsyncORM.interfaces;

namespace AsyncORM.DirectTable
{
    public class Table : ITable
    {
        private readonly string _connectionString;

        public Table(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<dynamic> InsertAsync(TableOperationSetting tableOperationSetting, dynamic entity)
        {
            return await Task.Run(async () =>
                                            {
                                                bool isIdentity =
                                                    tableOperationSetting.PrimaryKeys.Any(x => x.IsIdentity);
                                                if (isIdentity && tableOperationSetting.PrimaryKeys.Count > 1)
                                                {
                                                    throw new ArgumentException(
                                                        "There more than one primary key and at least one marked as Idenity true.");
                                                }
                                                if (String.IsNullOrWhiteSpace(tableOperationSetting.TableName))
                                                    throw new ArgumentException("Table name cannot be empty.");
                                                if (entity == null)
                                                    throw new ArgumentException("entity cannot be null.");

                                                string insertStatement =
                                                    await
                                                    PrepareInsertStatement(tableOperationSetting, entity, isIdentity);
                                                IQueryAsync query = new DynamicQuery(_connectionString);
                                                if (isIdentity)
                                                    return
                                                        await
                                                        query.ExecuteScalarAsync(insertStatement, entity);
                                                await
                                                    query.ExecuteNonQueryAsync(insertStatement, entity);
                                                return null;
                                            });
        }

        public async Task UpdateAsync(TableOperationSetting tableOperationSetting, dynamic entity)
        {
            await Task.Run(async () =>
                                     {
                                         if (String.IsNullOrWhiteSpace(tableOperationSetting.Where) &&
                                             !tableOperationSetting.PrimaryKeys.Any())
                                         {
                                             throw new ArgumentException(
                                                 "No primary key or where statement found. please provide at least one");
                                         }
                                         if (String.IsNullOrWhiteSpace(tableOperationSetting.TableName))
                                             throw new ArgumentException("Table name cannot be empty.");
                                         if (entity == null)
                                             throw new ArgumentException("entity cannot be null.");

                                         string updateStatement =
                                             await PrepareUpdateStatement(tableOperationSetting, entity);
                                         IQueryAsync query = new DynamicQuery(_connectionString);
                                         await query.ExecuteNonQueryAsync(updateStatement, entity);
                                     });
        }

        public async Task DeleteAsync(TableOperationSetting tableOperationSetting, dynamic entity)
        {
            await Task.Run(async () =>
                                     {
                                         if (String.IsNullOrWhiteSpace(tableOperationSetting.Where) &&
                                             !tableOperationSetting.PrimaryKeys.Any())
                                         {
                                             throw new ArgumentException(
                                                 "No primary key or where statement found. please provide at least one");
                                         }
                                         if (String.IsNullOrWhiteSpace(tableOperationSetting.TableName))
                                             throw new ArgumentException("Table name cannot be empty.");
                                         if (entity == null)
                                             throw new ArgumentException("entity cannot be null.");

                                         string deleteStatement =
                                             await PrepareDeleteStatement(tableOperationSetting);
                                         IQueryAsync query = new DynamicQuery(_connectionString);
                                         await query.ExecuteNonQueryAsync(deleteStatement, entity);
                                     });
        }

        private async Task<string> PrepareInsertStatement(TableOperationSetting tableOperationSetting, dynamic entity,
                                                          bool isIdentity)
        {
            return await Task.Run(() =>
                                      {
                                          IPrimaryKey primaryKey = null;
                                          if (isIdentity)
                                          {
                                              primaryKey =
                                                  tableOperationSetting.PrimaryKeys.FirstOrDefault(x => x.IsIdentity);
                                          }
                                          PropertyInfo[] props = entity.GetType().GetProperties();
                                          var queryBuilder = new StringBuilder();
                                          var valueBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("INSERT INTO {0} (", tableOperationSetting.TableName);
                                          valueBuilder.Append("VALUES (");
                                          int length = props.Length;
                                          for (int index = 0; index < length; index++)
                                          {
                                              PropertyInfo item = props[index];
                                              if (primaryKey != null &&
                                                  String.Compare(item.Name, primaryKey.Name,
                                                                 StringComparison.CurrentCultureIgnoreCase) == 0)
                                                  continue;

                                              queryBuilder.AppendFormat("{0},", item.Name);
                                              valueBuilder.AppendFormat("@{0},", item.Name);
                                          }
                                          queryBuilder.Append(")");
                                          valueBuilder.Append(")");
                                          queryBuilder.Replace(",)", ")");
                                          valueBuilder.Replace(",)", ")");
                                          if (isIdentity)
                                              valueBuilder.Append(";SELECT SCOPE_IDENTITY();");

                                          return queryBuilder.Append(valueBuilder).ToString();
                                      });
        }

        private async Task<string> PrepareUpdateStatement(TableOperationSetting tableOperationSetting, dynamic entity)
        {
            return await Task.Run(() =>
                                      {
                                          PropertyInfo[] props = entity.GetType().GetProperties();
                                          var queryBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("UPDATE {0} ", tableOperationSetting.TableName);
                                          queryBuilder.Append("SET ");
                                          int length = props.Length;
                                          for (int index = 0; index < length; index++)
                                          {
                                              PropertyInfo item = props[index];
                                              queryBuilder.AppendFormat("{0}=@{0},", item.Name);
                                          }

                                          string query = queryBuilder.ToString().TrimEnd(',');
                                          queryBuilder = new StringBuilder(query);
                                          if (!String.IsNullOrWhiteSpace(tableOperationSetting.Where))
                                          {
                                              queryBuilder.AppendFormat(" WHERE {0}", tableOperationSetting.Where);
                                              return queryBuilder.ToString();
                                          }
                                         
                                          queryBuilder.Append(" WHERE ");
                                          foreach (IPrimaryKey primaryKey in tableOperationSetting.PrimaryKeys)
                                          {
                                              queryBuilder.Replace(String.Format("{0}=@{0},", primaryKey.Name),
                                                                   string.Empty);
                                              queryBuilder.AppendFormat("{0}=@{0} &&", primaryKey.Name);
                                          }
                                          return queryBuilder.ToString().TrimEnd('&');
                                      });
        }

        private async Task<string> PrepareDeleteStatement(TableOperationSetting tableOperationSetting)
        {
            return await Task.Run(() =>
                                      {
                                          var queryBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("DELETE FROM {0} ", tableOperationSetting.TableName);

                                          if (!String.IsNullOrWhiteSpace(tableOperationSetting.Where))
                                          {
                                              queryBuilder.AppendFormat(" WHERE {0}", tableOperationSetting.Where);
                                              return queryBuilder.ToString();
                                          }
                                          queryBuilder.Append(" WHERE ");
                                          foreach (IPrimaryKey primaryKey in tableOperationSetting.PrimaryKeys)
                                          {
                                              queryBuilder.AppendFormat("{0}=@{0} &&", primaryKey.Name);
                                          }
                                          return queryBuilder.ToString().TrimEnd('&');
                                      });
        }
    }
}