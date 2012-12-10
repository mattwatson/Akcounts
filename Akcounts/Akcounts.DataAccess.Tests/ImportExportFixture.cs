using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;
using Akcounts.DataAccess;
using NHibernate;

namespace Akcounts.DataAccess.Tests
{
    [TestClass]
    public class ImportFromExcel_Fixture : TestFixtureBase
    {
        #region crap that comes in the file
        public ImportFromExcel_Fixture()
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

                protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        private void CreateInitialData()
        {

        }

        [TestMethod]
        public void Can_import_and_export_Excel()
        {
            ImportExport.ImportFromExcelFile("D:\\Dev\\WatsonFinancials\\Genesis Data\\Genesis Data.xls");
            ImportExport.ExportToExcelFile("D:\\Dev\\WatsonFinancials\\Genesis Data\\ExportTest.xls");

            //TODO Add Counts in here
        }

        [TestMethod]
        public void Can_import_from_the_export()
        {
            ImportExport.ImportFromExcelFile("D:\\Dev\\WatsonFinancials\\Genesis Data\\ExportTest.xls");

            //TODO Add Counts in here
        }
    }
}
