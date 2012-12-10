using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Akcounts.DataAccess.Tests
{
    [TestClass]
    public class TestFixtureBase
    {

        #region crap that comes in the file
        public TestFixtureBase()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
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

        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;

        protected ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        public void TestFixtureSetUp()
        {

            _configuration = new Configuration();

            _configuration.Configure();

            _configuration.Properties[NHibernate.Cfg.Environment.ConnectionProvider] =
                "NHibernate.Connection.DriverConnectionProvider";
            _configuration.Properties[NHibernate.Cfg.Environment.Dialect] =
                "NHibernate.Dialect.MsSql2005Dialect";
            _configuration.Properties[NHibernate.Cfg.Environment.ConnectionDriver] =
                "NHibernate.Driver.SqlClientDriver";
            _configuration.Properties[NHibernate.Cfg.Environment.ConnectionString] =
                "Data Source=192.168.80.32;Initial Catalog=Akcounts_Test;User ID=WF; Password=Wtgmp1abF;";

            _configuration.AddAssembly(typeof(Account).Assembly);

            _sessionFactory = _configuration.BuildSessionFactory();
        }

        public static void TestFixtureTearDown()
        {
            _sessionFactory.Close();
        }

        [TestInitialize]
        public void SetupContext()
        {
            if (_configuration == null) TestFixtureSetUp();
            new SchemaExport(_configuration).Execute(false, true, false, false);
            Before_each_test();
        }

        [TestCleanup]
        public void TearDownContext()
        {
            After_each_test();
            if (_sessionFactory != null) TestFixtureTearDown();
        }

        protected virtual void Before_each_test()
        { }

        protected virtual void After_each_test()
        { }

    }
}