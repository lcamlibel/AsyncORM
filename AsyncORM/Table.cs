﻿using System;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public Table()
        {
            if (String.IsNullOrEmpty(AsyncOrmConfig.ConnectionString))
                throw new ArgumentNullException(
                    "please setup global connectionstring use the following: AsyncOrmConfig.ConnectionString");
            _connectionString = AsyncOrmConfig.ConnectionString;
        }

        public async Task<dynamic> InsertAsync(dynamic entity, TableSetting tableSetting)
        {
            bool isIdentity = tableSetting.PrimaryKeys.Any(x => x.IsIdentity);
            if (isIdentity && tableSetting.PrimaryKeys.Count > 1)
            {
                throw new ArgumentException("There more than one primary key and at least one marked as Idenity true.");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string insertStatement = PrepareInsertStatement(tableSetting, entity, isIdentity);
            IQueryAsync query = new DynamicQuery(_connectionString);
            if (isIdentity)
                return await query.ExecuteScalarAsync(insertStatement, entity);
            await query.ExecuteNonQueryAsync(insertStatement, entity);
            return null;
        }

        public async Task UpdateAsync(dynamic entity, TableSetting tableSetting, string where = null)
        {
            if (String.IsNullOrWhiteSpace(where) && !tableSetting.PrimaryKeys.Any())
            {
                throw new ArgumentException("No primary key or where statement found. please provide at least one");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string updateStatement = PrepareUpdateStatement(tableSetting, entity, where);
            IQueryAsync query = new DynamicQuery(_connectionString);
            await query.ExecuteNonQueryAsync(updateStatement, entity);
        }

        public async Task DeleteAsync(dynamic entity, TableSetting tableSetting, string where = null)
        {
            if (String.IsNullOrWhiteSpace(where) && !tableSetting.PrimaryKeys.Any())
            {
                throw new ArgumentException("No primary key or where statement found. please provide at least one");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string deleteStatement = PrepareDeleteStatement(tableSetting, where);
            IQueryAsync query = new DynamicQuery(_connectionString);
            await query.ExecuteNonQueryAsync(deleteStatement, entity);
        }

        public async Task<T> InsertAsync<T>(dynamic entity, TableSetting tableSetting)
        {
            bool isIdentity = tableSetting.PrimaryKeys.Any(x => x.IsIdentity);
            if (isIdentity && tableSetting.PrimaryKeys.Count > 1)
            {
                throw new ArgumentException("There more than one primary key and at least one marked as Idenity true.");
            }
            if (String.IsNullOrWhiteSpace(tableSetting.TableName))
                throw new ArgumentException("Table name cannot be empty.");
            if (entity == null)
                throw new ArgumentException("entity cannot be null.");

            string insertStatement = await PrepareInsertStatement(tableSetting, entity, isIdentity);
            IQueryAsync query = new DynamicQuery(_connectionString);
            if (isIdentity)
                return await query.ExecuteScalarAsync(insertStatement, entity);
            await query.ExecuteNonQueryAsync(insertStatement, entity);
            return default(T);
        }

        private string PrepareInsertStatement(TableSetting tableSetting, dynamic entity,
            bool isIdentity)
        {
            IPrimaryKey primaryKey = null;
            if (isIdentity)
            {
                primaryKey = tableSetting.PrimaryKeys.FirstOrDefault(x => x.IsIdentity);
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
                    String.Compare(item.Name, primaryKey.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
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
        }

        private string PrepareUpdateStatement(TableSetting tableSetting, dynamic entity, string where)
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
                        queryBuilder.Replace(String.Format("{0}=@{0},", primaryKey.Name), string.Empty);
                    }
                }
                return queryBuilder.ToString();
            }
            if (tableSetting.PrimaryKeys == null)
                throw new ArgumentException("Primary Key cannot be empty.");

            queryBuilder.Append(" WHERE ");
            foreach (IPrimaryKey primaryKey in tableSetting.PrimaryKeys)
            {
                queryBuilder.Replace(String.Format("{0}=@{0},", primaryKey.Name), string.Empty);
                queryBuilder.AppendFormat("{0}=@{0} &&", primaryKey.Name);
            }
            return queryBuilder.ToString().TrimEnd('&');
        }

        private string PrepareDeleteStatement(TableSetting tableSetting, string where)
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
        }
    }
}