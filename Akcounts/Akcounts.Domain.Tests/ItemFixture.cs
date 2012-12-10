using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;

namespace Akcounts.Domain.Tests
{
    [TestClass]
    public class ItemFixture
    {

        #region crap that comes in the file
        public ItemFixture()
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
  
        private Account _account1;
        private Account _account2;
        private Account _account3;
        private Account _account4;
        private AccountType _accountType1;
        private AccountType _accountType2;
        private AccountType _accountType3;
        private AccountType _accountType4;
        private AccountCategory _accountCategory;
        private Transaction _transaction1;
        private Transaction _transaction2;
        private Transaction _transaction3;

        [TestInitialize]
        public void TestFixtureSetUp()
        {
            _accountType1 = new AccountType
            {
                Name = "Asset",
                IsDestination = true,
                IsSource = true,
                IsValid = true,
            };
            _accountType2 = new AccountType
            {
                Name = "Expense",
                IsDestination = true,
                IsSource = true,
                IsValid = true,
            };
            _accountType3 = new AccountType
            {
                Name = "NotSource",
                IsDestination = true,
                IsSource = false,
                IsValid = true,
            };
            _accountType4 = new AccountType
            {
                Name = "NotDestination",
                IsDestination = false,
                IsSource = true,
                IsValid = true,
            };
            _accountCategory = new AccountCategory
            {
                Name = "Other",
                Colour = "Orange",
                IsValid = true,
            };
            _account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            _account1.setType(_accountType1);
            _account1.setCategory(_accountCategory);

            _account2 = new Account
            {
                Name = "Food",
                IsValid = true,
            };
            _account2.setType(_accountType2);
            _account2.setCategory(_accountCategory);

            _account3 = new Account
            {
                Name = "Not Source",
                IsValid = true,
            };
            _account3.setType(_accountType3);
            _account3.setCategory(_accountCategory);

            _account4 = new Account
            {
                Name = "Not Destination",
                IsValid = true,
            };
            _account4.setType(_accountType4);
            _account4.setCategory(_accountCategory);

            DateTime tempDate = new DateTime(2009, 2, 5);
            _transaction1 = new Transaction
            {
                Date = tempDate,
                Description = "Some payment",
                IsVerified = false,
            };

            _transaction2 = new Transaction
            {
                Date = tempDate,
                Description = "Some other payment",
                IsVerified = true,
            };

            _transaction3 = new Transaction
            {
                Date = tempDate,
                Description = "A further payment",
                IsVerified = true,
            };
        }

        [TestMethod]
        public void Can_create_new_item()
        {
            var item = new Item
            {
                Value = 0.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSource(_account1);
            item.SetDestination(_account2);

            Assert.IsNotNull(item);
            Assert.AreEqual(item.Id, Guid.Empty);
            Assert.AreEqual(item.Source.Id, Guid.Empty);
            Assert.AreEqual(item.Source.Category.Id, Guid.Empty);
            Assert.AreEqual(item.Source.Type.Id, Guid.Empty);
            Assert.AreEqual(item.Destination.Id, Guid.Empty);
            Assert.AreEqual(item.Destination.Category.Id, Guid.Empty);
            Assert.AreEqual(item.Destination.Type.Id, Guid.Empty);
            Assert.AreEqual(item.TransactionId.Id, Guid.Empty);

            Assert.AreEqual(item.Description, "Add Test");
            Assert.AreEqual(item.Destination, _account2);
            Assert.AreEqual(item.IsVerified, true);
            Assert.AreEqual(item.Source, _account1);
            Assert.AreEqual(item.TransactionId, _transaction1);
            Assert.AreEqual(item.Value, 0.01M);
            Assert.AreEqual(item.Destination.Category, _account2.Category);
            Assert.AreEqual(item.Destination.Category.Colour, _account2.Category.Colour);
            Assert.AreEqual(item.Destination.Category.IsValid, _account2.Category.IsValid);
            Assert.AreEqual(item.Destination.Category.Name, _account2.Category.Name);
            Assert.AreEqual(item.Destination.IsValid, _account2.IsValid);
            Assert.AreEqual(item.Destination.Name, _account2.Name);
            Assert.AreEqual(item.Destination.Type, _account2.Type);
            Assert.AreEqual(item.Destination.Type.IsDestination, _account2.Type.IsDestination);
            Assert.AreEqual(item.Destination.Type.IsSource, _account2.Type.IsSource);
            Assert.AreEqual(item.Destination.Type.IsValid, _account2.Type.IsValid);
            Assert.AreEqual(item.Destination.Type.Name, _account2.Type.Name);
            Assert.AreEqual(item.Source.Category, _account1.Category);
            Assert.AreEqual(item.Source.Category.Colour, _account1.Category.Colour);
            Assert.AreEqual(item.Source.Category.IsValid, _account1.IsValid);
            Assert.AreEqual(item.Source.Category.Name, _account1.Category.Name);
            Assert.AreEqual(item.Source.IsValid, _account1.IsValid);
            Assert.AreEqual(item.Source.Name, _account1.Name);
            Assert.AreEqual(item.Source.Type, _account1.Type);
            Assert.AreEqual(item.Source.Type.IsDestination, _account1.Type.IsDestination);
            Assert.AreEqual(item.Source.Type.IsSource, _account1.Type.IsSource);
            Assert.AreEqual(item.Source.Type.IsValid, _account1.Type.IsValid);
            Assert.AreEqual(item.Source.Type.Name, _account1.Type.Name);

        }

        [TestMethod]
        [ExpectedException(typeof(ItemSourceEqualDestinationException))]
        public void Cannot_move_money_within_the_same_account()
        {
            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSource(_account1);
            item.SetDestination(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemSourceEqualDestinationException))]
        public void Cannot_move_money_within_the_same_account2()
        {
            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetDestination(_account1);
            item.SetSource(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemInvalidSourceException))]
        public void Cannot_move_money_from_destination_only_account()
        {
            var item = new Item
            {
                Value = 99.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSource(_account3);
            item.SetDestination(_account1);
        }

        [TestMethod]        
        [ExpectedException(typeof(ItemInvalidDestinationException))]
        public void Cannot_move_money_into_source_only_account()
        {
            var item = new Item
            {
                Value = 88.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSource(_account1);
            item.SetDestination(_account4);
        }

        [TestMethod]
        public void TransactionUpdatedIfItemMoved()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction3);
            item.SetDestination(_account1);
            item.SetSource(_account2);
            Assert.AreEqual(item.TransactionId, _transaction3);
            Assert.AreNotEqual(item.TransactionId, _transaction2);
            Assert.AreEqual(1, _transaction3.ItemCount());
            Assert.AreEqual(0, _transaction2.ItemCount());

            item.SetTransaction(_transaction2);
            Assert.AreEqual(item.TransactionId, _transaction2);
            Assert.AreNotEqual(item.TransactionId, _transaction3);
            Assert.AreEqual(0, _transaction3.ItemCount());
            Assert.AreEqual(1, _transaction2.ItemCount());

        }

        [TestMethod]
        public void SourceUpdatedIfItemMoved()
        {
            Account account1;
            Account account2;

            account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            account1.setType(_accountType1);
            account1.setCategory(_accountCategory);

            account2 = new Account
            {
                Name = "Food",
                IsValid = true,
            };
            account2.setType(_accountType2);
            account2.setCategory(_accountCategory);

            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };

            item.SetSource(account2);
            Assert.AreEqual(item.Source, account2);
            Assert.AreNotEqual(item.Source, account1);
            Assert.AreEqual(1, account2.ItemsSourceCount());
            Assert.AreEqual(0, account1.ItemsSourceCount());

            item.SetSource(account1);
            Assert.AreEqual(item.Source, account1);
            Assert.AreNotEqual(item.Source, account2);
            Assert.AreEqual(1, account1.ItemsSourceCount());
            Assert.AreEqual(0, account2.ItemsSourceCount());

        }

        [TestMethod]
        public void DestinationUpdatedIfItemMoved()
        {
            Account account1;
            Account account3;

            account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            account1.setType(_accountType1);
            account1.setCategory(_accountCategory);

            account3 = new Account
            {
                Name = "Not Source",
                IsValid = true,
            };
            account3.setType(_accountType3);
            account3.setCategory(_accountCategory);

            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };

            item.SetDestination(account3);
            Assert.AreEqual(item.Destination, account3);
            Assert.AreNotEqual(item.Destination, account1);
            Assert.AreEqual(1, account3.ItemsDestinationCount());
            Assert.AreEqual(0, account1.ItemsDestinationCount());

            item.SetDestination(account1);
            Assert.AreEqual(item.Destination, account1);
            Assert.AreNotEqual(item.Destination, account3);
            Assert.AreEqual(1, account1.ItemsDestinationCount());
            Assert.AreEqual(0, account3.ItemsDestinationCount());

        }



        [TestMethod]
        public void SourceUpdatedIfItemMovedLazily()
        {
            Account account1;
            Account account2;

            account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            account1.setType(_accountType1);
            account1.setCategory(_accountCategory);

            account2 = new Account
            {
                Name = "Food",
                IsValid = true,
            };
            account2.setType(_accountType2);
            account2.setCategory(_accountCategory);

            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };

            item.SetSourceLazy(account2);
            Assert.AreEqual(item.Source, account2);
            Assert.AreNotEqual(item.Source, account1);

            item.SetSourceLazy(account1);
            Assert.AreEqual(item.Source, account1);
            Assert.AreNotEqual(item.Source, account2);

        }

        [TestMethod]
        public void DestinationUpdatedIfItemMovedLazily()
        {
            Account account1;
            Account account3;

            account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            account1.setType(_accountType1);
            account1.setCategory(_accountCategory);

            account3 = new Account
            {
                Name = "Not Source",
                IsValid = true,
            };
            account3.setType(_accountType3);
            account3.setCategory(_accountCategory);

            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };

            item.SetDestinationLazy(account3);
            Assert.AreEqual(item.Destination, account3);
            Assert.AreNotEqual(item.Destination, account1);

            item.SetDestinationLazy(account1);
            Assert.AreEqual(item.Destination, account1);
            Assert.AreNotEqual(item.Destination, account3);

        }

        [TestMethod]
        public void Transaction_desc_works()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            Assert.AreEqual("Move Test", item.Description);
            Assert.AreEqual("Move Test", item.TransactionDesc);

            item.SetTransaction(_transaction1);
            Assert.AreEqual("Some payment", _transaction1.Description);
            Assert.AreEqual("Some payment: Move Test", item.TransactionDesc);

            _transaction1.Description = "";
            Assert.AreEqual("Move Test", item.TransactionDesc);
        }

        [TestMethod]
        public void Item_SourceName()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            item.SetSource(_account1);
            Assert.AreEqual(item.SourceName, "Bank Account");
        }

        [TestMethod]
        public void Item_DestinationName()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            item.SetDestination(_account1);
            Assert.AreEqual(item.DestinationName, "Bank Account");
        }

        [TestMethod]
        public void Transaction_TransactionDate()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            Assert.AreEqual(item.TransactionDate, "05 February 2009");
        }

        [TestMethod]
        public void Transaction_TVerfied()
        {
            var item = new Item
            {
                Value = 99.99M,
                Description = "Move Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            Assert.AreEqual(item.TVerified, false);

            item.TVerified = true;
            Assert.AreEqual(item.TVerified, true);
            Assert.AreEqual(_transaction1.IsVerified, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemSourceEqualDestinationException))]
        public void Cannot_move_money_within_the_same_account_lazily()
        {
            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSourceLazy(_account1);
            item.SetDestinationLazy(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemSourceEqualDestinationException))]
        public void Cannot_move_money_within_the_same_account2_lazily()
        {
            var item = new Item
            {
                Value = 567.78M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetDestinationLazy(_account1);
            item.SetSourceLazy(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemInvalidSourceException))]
        public void Cannot_move_money_from_destination_only_account_lazily()
        {
            var item = new Item
            {
                Value = 99.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSourceLazy(_account3);
            item.SetDestinationLazy(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemInvalidDestinationException))]
        public void Cannot_move_money_into_source_only_account_lazily()
        {
            var item = new Item
            {
                Value = 88.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction1);
            item.SetSourceLazy(_account1);
            item.SetDestinationLazy(_account4);
        }
    }
}
