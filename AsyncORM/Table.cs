using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.interfaces;

namespace AsyncORM
{
    public class Table : ITable
    {
        private readonly string _connectionString;

        public Table(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<dynamic> InsertAsync(dynamic entity, TableSetting tableSetting,
                                               CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<dynamic>(async () =>
                                                     {
                                                         bool isIdentity =
                                                             tableSetting.PrimaryKeys.Any(x => x.IsIdentity);
                                                         if (isIdentity && tableSetting.PrimaryKeys.Count > 1)
                                                         {
                                                             throw new ArgumentException(
                                                                 "There more than one primary key and at least one marked as Idenity true.");
                                                         }
                                                         if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                                                             throw new ArgumentException("Table name cannot be empty.");
                                                         if (entity == null)
                                                             throw new ArgumentException("entity cannot be null.");

                                                         string insertStatement =
                                                             await
                                                             PrepareInsertStatementAsync(tableSetting, entity,
                                                                                         isIdentity, cancellationToken);
                                                         IQueryAsync query = new DynamicQuery(_connectionString);
                                                         if (isIdentity)
                                                             return
                                                                 await
                                                                 query.ExecuteScalarAsync(insertStatement, entity,
                                                                                          cancellationToken:
                                                                                              cancellationToken);
                                                         await
                                                             query.ExecuteNonQueryAsync(insertStatement, entity,
                                                                                        cancellationToken:
                                                                                            cancellationToken);
                                                         return null;
                                                     }, cancellationToken);
        }

        public async Task UpdateAsync(dynamic entity, TableSetting tableSetting, string where = null,
                                      CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(where) && !tableSetting.PrimaryKeys.Any())
            {
                throw new ArgumentException(
                    "No primary key or where statement found. please provide at least one");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string updateStatement =
                await PrepareUpdateStatementAsync(tableSetting, entity, where, cancellationToken);
            IQueryAsync query = new DynamicQuery(_connectionString);
            await query.ExecuteNonQueryAsync(updateStatement, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(TableSetting tableSetting, dynamic entity, string where = null,
                                      CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(where) && !tableSetting.PrimaryKeys.Any())
            {
                throw new ArgumentException(
                    "No primary key or where statement found. please provide at least one");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string deleteStatement =
                await
                PrepareDeleteStatementAsync(tableSetting, where, cancellationToken);
            IQueryAsync query = new DynamicQuery(_connectionString);
            await
                query.ExecuteNonQueryAsync(deleteStatement, entity,
                                           cancellationToken: cancellationToken);
        }

        public async Task<T> InsertAsync<T>(dynamic entity, TableSetting tableSetting,
                                            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<dynamic>(async () =>
                                                     {
                                                         bool isIdentity =
                                                             tableSetting.PrimaryKeys.Any(x => x.IsIdentity);
                                                         if (isIdentity && tableSetting.PrimaryKeys.Count > 1)
                                                         {
                                                             throw new ArgumentException(
                                                                 "There more than one primary key and at least one marked as Idenity true.");
                                                         }
                                                         if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                                                             throw new ArgumentException("Table name cannot be empty.");
                                                         if (entity == null)
                                                             throw new ArgumentException("entity cannot be null.");

                                                         string insertStatement =
                                                             await
                                                             PrepareInsertStatementAsync(tableSetting, entity,
                                                                                         isIdentity, cancellationToken);
                                                         IQueryAsync query = new DynamicQuery(_connectionString);
                                                         if (isIdentity)
                                                             return
                                                                 await
                                                                 query.ExecuteScalarAsync(insertStatement, entity,
                                                                                          cancellationToken:
                                                                                              cancellationToken);
                                                         await
                                                             query.ExecuteNonQueryAsync(insertStatement, entity,
                                                                                        cancellationToken:
                                                                                            cancellationToken);
                                                         return null;
                                                     }, cancellationToken);
        }

        private async Task<string> PrepareInsertStatementAsync(TableSetting tableSetting, dynamic entity,
                                                               bool isIdentity,
                                                               CancellationToken cancellationToken =
                                                                   default(CancellationToken))
        {
            return await Task.Run(() =>
                                      {
                                          IPrimaryKey primaryKey = null;
                                          if (isIdentity)
                                          {
                                              primaryKey =
                                                  tableSetting.PrimaryKeys.FirstOrDefault(x => x.IsIdentity);
                                          }
                                          PropertyInfo[] props = entity.GetType().GetProperties();
                                          var queryBuilder = new StringBuilder();
                                          var valueBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("INSERT INTO {0} (", tableSetting.TableName);
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
                                      }, cancellationToken);
        }

        private async Task<string> PrepareUpdateStatementAsync(TableSetting tableSetting, dynamic entity, string where,
                                                               CancellationToken cancellationToken =
                                                                   default(CancellationToken))
        {
            return await Task.Run(() =>
                                      {
                                          PropertyInfo[] props = entity.GetType().GetProperties();
                                          var queryBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("UPDATE {0} ", tableSetting.TableName);
                                          queryBuilder.Append("SET ");
                                          int length = props.Length;
                                          for (int index = 0; index < length; index++)
                                          {
                                              PropertyInfo item = props[index];
                                              queryBuilder.AppendFormat("{0}=@{0},", item.Name);
                                          }

                                          string query = queryBuilder.ToString().TrimEnd(',');
                                          queryBuilder = new StringBuilder(query);
                                          if (!String.IsNullOrWhiteSpace(where))
                                          {
                                              queryBuilder.AppendFormat(" WHERE {0}", where);
                                              if (tableSetting.PrimaryKeys != null)
                                              {
                                                  foreach (IPrimaryKey primaryKey in tableSetting.PrimaryKeys)
                                                  {
                                                      queryBuilder.Replace(String.Format("{0}=@{0},", primaryKey.Name),
                                                                           string.Empty);
                                                  }
                                              }
                                              return queryBuilder.ToString();
                                          }
                                          if (tableSetting.PrimaryKeys == null)
                                              throw new ArgumentException("Primary Key cannot be empty.");

                                          queryBuilder.Append(" WHERE ");
                                          foreach (IPrimaryKey primaryKey in tableSetting.PrimaryKeys)
                                          {
                                              queryBuilder.Replace(String.Format("{0}=@{0},", primaryKey.Name),
                                                                   string.Empty);
                                              queryBuilder.AppendFormat("{0}=@{0} &&", primaryKey.Name);
                                          }
                                          return queryBuilder.ToString().TrimEnd('&');
                                      }, cancellationToken);
        }

        private async Task<string> PrepareDeleteStatementAsync(TableSetting tableSetting, string where,
                                                               CancellationToken cancellationToken =
                                                                   default(CancellationToken))
        {
            return await Task.Run(() =>
                                      {
                                          var queryBuilder = new StringBuilder();
                                          queryBuilder.AppendFormat("DELETE FROM {0} ", tableSetting.TableName);

                                          if (!String.IsNullOrWhiteSpace(where))
                                          {
                                              queryBuilder.AppendFormat(" WHERE {0}", where);
                                              return queryBuilder.ToString();
                                          }
                                          queryBuilder.Append(" WHERE ");
                                          foreach (IPrimaryKey primaryKey in tableSetting.PrimaryKeys)
                                          {
                                              queryBuilder.AppendFormat("{0}=@{0} &&", primaryKey.Name);
                                          }
                                          return queryBuilder.ToString().TrimEnd('&');
                                      }, cancellationToken);
        }
    }
}