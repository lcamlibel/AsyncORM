using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using AsyncORM.interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncORM.UnitTests
{
    [TestClass]
    public class QueryAsyncFactoryTest
    {
        [TestInitialize()]
        public void Startup()
        {
            AsyncOrmConfig.ConnectionString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
        }
        [TestMethod]
        public void QueryAsyncFactory_DynamicQueryVerify()
        {
            QueryAsyncFactory factory = new QueryAsyncFactory();
             string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
             IQueryAsync query = factory.Create(Library.QueryAsyncType.DynamicQuery, connString);
             Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        [TestMethod]
        public void QueryAsyncFactory_StoredProcedureVerify()
        {
            QueryAsyncFactory factory = new QueryAsyncFactory();
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync query = factory.Create(Library.QueryAsyncType.StoredProcedure, connString);
            Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        public void QueryAsyncFactory_Instance_DynamicQueryVerify()
        {            
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync query = QueryAsyncFactory.Instance.Create(Library.QueryAsyncType.DynamicQuery, connString);
            Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        [TestMethod]
        public void QueryAsyncFactory_Intance_StoredProcedureVerify()
        {            
            string connString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            IQueryAsync query = QueryAsyncFactory.Instance.Create(Library.QueryAsyncType.StoredProcedure, connString);
            Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        [TestMethod]
        public void QueryAsyncFactory_DynamicQuery_With_GlobalConfiguration_Verify()
        {
            QueryAsyncFactory factory = new QueryAsyncFactory();
            
            var query = factory.Create(Library.QueryAsyncType.DynamicQuery);
            Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        [TestMethod]
        public void QueryAsyncFactory_StoredProcedure_With_GlobalConfiguration_Verify()
        {
            QueryAsyncFactory factory = new QueryAsyncFactory();
            IQueryAsync query = factory.Create(Library.QueryAsyncType.StoredProcedure);
            Assert.IsInstanceOfType(query, typeof(IQueryAsync));
        }
        [TestMethod]
        public async Task QueryAsyncFactory_DynamicQuery_Run()
        {
            QueryAsyncFactory factory = new QueryAsyncFactory();

            IQueryAsync dynamicQuery = factory.Create(Library.QueryAsyncType.DynamicQuery);
            IEnumerable<dynamic> result =
               await dynamicQuery.ExecuteAsync("select top 1 * from [HumanResources].[Department]");
            Assert.IsTrue(result.Any());
        }
        [TestMethod]
        public async Task QueryAsyncFactory_StoredProcedure_Run()
        {
            IQueryAsyncFactory factory = new QueryAsyncFactory();
            IQueryAsync storedProcedure = factory.Create(Library.QueryAsyncType.StoredProcedure);
            IEnumerable<dynamic> result =
                  await
                  storedProcedure.ExecuteAsync("proc_test2");
            Assert.IsTrue(result.Any());
        }
    }
}
