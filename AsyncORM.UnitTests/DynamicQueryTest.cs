using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
                    new { DepartmentId = 8 });
            Assert.IsTrue(result.Any(x => x.DepartmentID==8));
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
    }
}