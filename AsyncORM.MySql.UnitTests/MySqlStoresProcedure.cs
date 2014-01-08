using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncORM.MySql.Extensions;
using AsyncORM.MySql.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncORM.MySql.UnitTests
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
    }
    [TestClass]
    public class MySqlStoresProcedure
    {
        [TestMethod]
        public async Task Verify_StoredProcedure()
        {
            string connString = "";
            var storedProcedure = new MySqlStoredProcedure(connString);
            var courses = await storedProcedure.ExecuteAsync<Course>("test", new { t = 1 });
           
            Assert.AreEqual(courses.ElementAt(0).CourseId,1);
        }
    }
}
