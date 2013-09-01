using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncORM.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncORM.UnitTests
{
    public class Address
    {
        public int AddressID { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }

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


            IEnumerable<dynamic> data = result.ElementAt(0);
            IEnumerable<dynamic> data1 = result.ElementAt(1);
            IEnumerable<dynamic> data2 = result.ElementAt(2);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data1.ElementAt(0).data == null);
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
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleResultSetAsync_GenericList()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            IEnumerable<Address> result =
                await
                storedProcedure.ExecuteAsync<Address>("proc_test3");
            Assert.IsTrue(result.Count() == 10);
        }

        [TestMethod]
        public async Task ExecuteAsync_Verify_Type()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            IEnumerable<Address> result =
                await
                storedProcedure.ExecuteAsync<Address>("proc_test3");
            Assert.IsInstanceOfType(result.ElementAt(0), typeof (Address));
        }

        [TestMethod]
        [ExpectedException(typeof (TaskCanceledException))]
        public async Task ExecuteAsync_CancelationToken()
        {
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync storedProcedure = new StoredProcedure(connString);
            var tokenSource = new CancellationTokenSource();

            Task<IEnumerable<Address>> task = storedProcedure.ExecuteAsync<Address>("proc_test3",
                                                                                    cancellationToken: tokenSource.Token);
            tokenSource.Cancel();
            await Task.WhenAll(task);
            Assert.IsTrue(!task.Result.Any());
        }
    }
}