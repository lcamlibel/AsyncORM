using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AsyncORM.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncORM.UnitTests
{
    [TestClass]
    public class StoredProcedureTest
    {
        [TestMethod]
        public async Task ExecuteMultipleAsyncExecuteMultipleResultSetAsync_Success_NoParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            IEnumerable<dynamic> result =
                await
                storedProcedure.ExecuteAsync("proc_test");


             IEnumerable<dynamic>  data = result.ElementAt(0);
             IEnumerable<dynamic> data1 = result.ElementAt(1);
             IEnumerable<dynamic> data2 = result.ElementAt(2);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data1.ElementAt(0).data==null);
            Assert.IsTrue(data2.Any());
        }
        [TestMethod]
        public async Task ExecuteAsync_SignleResultSetAsync_Success_NoParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            IEnumerable<dynamic> result =
                await
                storedProcedure.ExecuteAsync("proc_test2");
            Assert.IsTrue(result.Any());
            
        }
        [TestMethod]
        public async Task ExecuteAsync_SignleResultSetAsync_REturns1Result_NoParameter()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            IEnumerable<dynamic> result =
                await
                storedProcedure.ExecuteAsync("proc_test2");
            Assert.IsTrue(result.Count()==1);

        }
    }

}
