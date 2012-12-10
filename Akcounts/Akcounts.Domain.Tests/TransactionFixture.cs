using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;

namespace Akcounts.Domain.Tests
{
    [TestClass]
    public class TransactionFixture
    {

        #region crap that comes in the file
        public TransactionFixture()
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
  
        [TestMethod]
        public void Can_create_new_transaction()
        {

            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };

            var transaction = new Transaction { 
                BusinessKey = 1,
                Date = new DateTime(2009, 11, 15),
                Description = "Test Transaction",
                IsVerified = true,
                
            };
            item.SetTransaction(transaction);
                            
            Assert.IsNotNull(transaction);
            Assert.AreEqual(transaction.Id, Guid.Empty);
            Assert.AreEqual(transaction.ItemCount(), 1);
        }

        [TestMethod]
        public void ItemCountReturnsCorrectValues()
        {

            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };

            var item2 = new Item
            {
                Value = 667.78M,
                Description = "Add Test 2",
                IsVerified = false,
            };
            var transaction = new Transaction
            {
                BusinessKey = 1,
                Date = new DateTime(2009, 11, 15),
                Description = "Test Transaction",
                IsVerified = true,

            };
            Assert.AreEqual(transaction.ItemCount(), 0);

            item.SetTransaction(transaction);
            Assert.AreEqual(transaction.ItemCount(), 1);

            item.SetTransaction(transaction);
            Assert.AreEqual(transaction.ItemCount(), 1);
            
            item2.SetTransaction(transaction);
            Assert.AreEqual(transaction.ItemCount(), 2);

            var transaction2 = new Transaction
            {
                BusinessKey = 1,
                Date = new DateTime(2009, 11, 16),
                Description = "Test Transaction2",
                IsVerified = true,

            };
            Assert.AreEqual(transaction2.ItemCount(), 0);
            item.SetTransaction(transaction2);
            Assert.AreEqual(transaction.ItemCount(), 1);
            Assert.AreEqual(transaction2.ItemCount(), 1);
        }
    }
}
