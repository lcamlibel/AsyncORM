using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using AsyncORM.DirectTable;
using AsyncORM.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncORM.UnitTests
{
    [TestClass]
    public class TableTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            String tt = "select where rrt=4 &&";
            string r = tt.TrimEnd('&');
            Assert.AreEqual("select where rrt=4 ", r);

        }
        [TestMethod]
        public async Task InsertAsync_Sucess()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
                                             {
                                                 PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           }
                                                                   },
                                                                   TableName = "Customer"
                                             };
           ITable table=new Table(connString);
            await table.InsertAsync(setting, new {CustomerName = "Test", Address = "Address1"});

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithPrimaryKey()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
            {
                PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           }
                                                                   },
                TableName = "Customer"
            };
            ITable table = new Table(connString);
            await table.UpdateAsync(setting, new { CustomerId=1,CustomerName = "TestUpdate", Address = "Address Update" });

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithWhereStatement()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
            {
                TableName = "Customer",
                Where="CustomerId=@CustomerId"
                
            };
            ITable table = new Table(connString);
            await table.UpdateAsync(setting, new { CustomerName = "TestUpdate With Where", Address = "Address Update With Where" });

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithWhereStatement2()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
            {
                TableName = "Customer",
                Where = "CustomerName like '%Henry%'"

            };
            ITable table = new Table(connString);
            await table.UpdateAsync(setting, new { CustomerName = "TestUpdate With Henry", Address = "Address Update With Henry" });

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAsync_Sucess_WithEntityIsNUll()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
            {
                TableName = "Customer",
                Where = "CustomerName like '%Henry%'"

            };
            ITable table = new Table(connString);
            await table.UpdateAsync(setting, null);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task InsertAsync_Sucess_WithMultiplePrimaryKeyWithMultipleIsIdentityYTrue()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableOperationSetting setting = new TableOperationSetting
            {
                PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           },
                                                                            new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "LocationId"
                                                                           }
                                                                   },
                TableName = "Customer"
            };
            ITable table = new Table(connString);
            await table.InsertAsync(setting, new { CustomerId = 1, CustomerName = "TestUpdate", Address = "Address Update" });

        }
    }
}
