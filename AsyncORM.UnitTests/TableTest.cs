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
            TableSetting setting = new TableSetting
                                             {
                                                 PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           }
                                                                   },
                                                                   TableName = "TestCustomer"
                                             };
           ITable table=new Table(connString);
            await table.InsertAsync(new {CustomerName = "Test1", Address = "Address1", LocationId=1}, setting);

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithPrimaryKey()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableSetting setting = new TableSetting
            {
                PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           }
                                                                   },
                TableName = "TestCustomer"
            };
            ITable table = new Table(connString);
            await table.UpdateAsync(new { CustomerId=2,CustomerName = "TestUpdate2", Address = "Address Update2",LocationId=1 }, setting);

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithWhereStatement()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableSetting setting = new TableSetting
            {
                PrimaryKeys = new List<IPrimaryKey>
                                                                   {
                                                                       new PrimaryKey
                                                                           {
                                                                               IsIdentity = true,
                                                                               Name = "CustomerId"
                                                                           }
                                                                   },
                TableName = "TestCustomer",
                Where="CustomerId=@CustomerId"
                
            };
            ITable table = new Table(connString);
            await table.UpdateAsync(new {CustomerId=2, CustomerName = "TestUpdate With Where3", Address = "Address Update With Where3", LocationId = 1 }, setting);

        }
        [TestMethod]
        public async Task UpdateAsync_Sucess_WithWhereStatement2()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableSetting setting = new TableSetting
            {

                TableName = "TestCustomer",
                Where = "CustomerName like '%TestUpdat%'"

            };
            ITable table = new Table(connString);
            await table.UpdateAsync(new { CustomerName = "TestUpdate With Henry", Address = "Address Update With Henry" }, setting);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAsync_Sucess_WithEntityIsNUll()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableSetting setting = new TableSetting
            {
                TableName = "Customer",
                Where = "CustomerName like '%Henry%'"

            };
            ITable table = new Table(connString);
            await table.UpdateAsync(null, setting);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task InsertAsync_Sucess_WithMultiplePrimaryKeyWithMultipleIsIdentityYTrue()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            ;
            TableSetting setting = new TableSetting
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
            await table.InsertAsync(new { CustomerId = 1, CustomerName = "TestUpdate", Address = "Address Update" }, setting);

        }
    }
}
