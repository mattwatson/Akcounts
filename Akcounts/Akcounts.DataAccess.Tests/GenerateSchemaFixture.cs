using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Akcounts.DataAccess.Tests
{
    [TestClass]
    public class GenerateSchema_Fixture
    {

        #region crap that comes in the file
        public GenerateSchema_Fixture()
        {
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #endregion //crap that comes in the file
  
        [TestMethod]
        public void Can_generate_schema()
        {
            var cfg = new Configuration();

            cfg.Configure();
            cfg.AddAssembly(typeof(Item).Assembly);
            new SchemaExport(cfg).Execute(false, true, false, false);

        }
    }
}
