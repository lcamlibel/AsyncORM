using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncORM.UnitTests
{
    [TestClass]
    public class DynamicQueryTest
    {
        [TestMethod]
        public async Task ExecuteAsync_Success_NoParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<dynamic> result =
                await dynamicQuery.ExecuteAsync("select top 1 * from [HumanResources].[Department]");
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task ExecuteAsync_Success_WithParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<dynamic> result =
                await
                dynamicQuery.ExecuteAsync(
                    "select top 1 * from [HumanResources].[Department] where DepartmentId=@DepartmentId",
                    new {DepartmentId = 8});
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task ExecuteAsync_Verify_WithParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<dynamic> result =
                await
                dynamicQuery.ExecuteAsync(
                    "select top 1 * from [HumanResources].[Department] where DepartmentId=@DepartmentId",
                    new {DepartmentId = 8});
            Assert.IsTrue(result.Any(x => x.DepartmentID == 8));
        }

        [TestMethod]
        public async Task ExecuteAsync_Projection_NoParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<dynamic> result =
                await
                dynamicQuery.ExecuteAsync("select * from [HumanResources].[Department]");

            var departments = result.Select(x => new {DepartmentId = x.DepartmentID, DeptName = x.Name});
            Assert.IsTrue(departments.Any());
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleResultSetAsync_GenericList()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<Address> result =
                await
                dynamicQuery.ExecuteAsync<Address>("select top 10 * from [Person].[Address]");
            Assert.IsTrue(result.Count() == 10);
        }

        [TestMethod]
        public async Task ExecuteAsync_Verify_Type()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            IEnumerable<Address> result =
                await
                dynamicQuery.ExecuteAsync<Address>("select top 10 * from [Person].[Address]");
            Assert.IsInstanceOfType(result.ElementAt(0), typeof (Address));
        }

        [TestMethod]
        [ExpectedException(typeof (TaskCanceledException))]
        public async Task ExecuteAsync_CancelationToken()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            var tokenSource = new CancellationTokenSource();
            Task<IEnumerable<Address>> task =
                dynamicQuery.ExecuteAsync<Address>("select top 10 * from [Person].[Address]",
                                                   cancellationToken: tokenSource.Token);

            tokenSource.Cancel();
            await Task.WhenAll(task);

            Assert.IsTrue(!task.Result.Any());
        }
        [TestMethod]
        public async Task Verify_Global_ConnectionString()
        {
            AsyncOrmConfig.ConnectionString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery();
            IEnumerable<Address> result =
                           await
                           dynamicQuery.ExecuteAsync<Address>("select top 10 * from [Person].[Address]");
            Assert.IsInstanceOfType(result.ElementAt(0), typeof(Address));
        }
        [TestMethod]
        public void StoredProcedure_Verify_Interfaces()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync dynamicQuery = new DynamicQuery(connString);
            Assert.IsInstanceOfType(dynamicQuery, typeof(IQueryAsync));
            Assert.IsInstanceOfType(dynamicQuery, typeof(IDynamicQuery));

        }
    }
}