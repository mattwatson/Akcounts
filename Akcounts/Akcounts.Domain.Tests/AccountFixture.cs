using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;

namespace Akcounts.Domain.Tests
{
    [TestClass]
    public class AccountFixture
    {

        #region crap that comes in the file
        public AccountFixture()
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

        private AccountType _accountType1;
        private AccountType _accountType2;
        private AccountCategory _accountCategory;
        private Item _item;
        private Item _item2;

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
            _accountCategory = new AccountCategory
            {
                Name = "Other",
                Colour = "Orange",
                IsValid = true,
            };


            _item = new Item
            {
                Value = 0.01M,
                Description = "Add Test",
                IsVerified = true,
            };

            _item2 = new Item
            {
                Value = 100.01M,
                Description = "Item test",
                IsVerified = false,
            };
        }

        [TestMethod]
        public void Can_create_new_Account()
        {

            var account = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);

            Assert.IsNotNull(account);
            Assert.AreEqual(account.Id, Guid.Empty);
            Assert.AreEqual(account.Category.Id, Guid.Empty);
            Assert.AreEqual(account.Category.Colour, "Orange");
            Assert.AreEqual(account.Category.IsValid, true);
            Assert.AreEqual(account.Category.Name, "Other");
            Assert.AreEqual(account.IsValid, true);
            Assert.AreEqual(account.Name, "Bank Account");
            Assert.AreEqual(account.Type.Id, Guid.Empty);
            Assert.AreEqual(account.Type.IsDestination, true);
            Assert.AreEqual(account.Type.IsSource, true);
            Assert.AreEqual(account.Type.IsValid, true);
            Assert.AreEqual(account.Type.Name, "Asset");
        }

        [TestMethod]
        public void Count_Source_Items_returns_correct_result()
        {
            var account = new Account
            {
                Name = "Test Account",
                IsValid = true,

            };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);

            Assert.AreEqual(account.ItemsSourceCount(), 0);

            _item.SetSource(account);
            Assert.AreEqual(account.ItemsSourceCount(), 1);

            _item.SetSource(account);
            Assert.AreEqual(account.ItemsSourceCount(), 1);

            _item2.SetSource(account);
            Assert.AreEqual(account.ItemsSourceCount(), 2);

            var account2 = new Account
            {
                Name = "Test Account",
                IsValid = true,

            };
            account2.setType(_accountType1);
            account2.setCategory(_accountCategory);


            Assert.AreEqual(account2.ItemsSourceCount(), 0);
            _item.SetSource(account2);
            Assert.AreEqual(account.ItemsSourceCount(), 1);
            Assert.AreEqual(account2.ItemsSourceCount(), 1);
        }

        [TestMethod]
        public void Count_Destination_Items_returns_correct_result()
        {
            var account = new Account
            {
                Name = "Test Account",
                IsValid = true,

            };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);

            Assert.AreEqual(account.ItemsDestinationCount(), 0);

            _item.SetDestination(account);
            Assert.AreEqual(account.ItemsDestinationCount(), 1);

            _item.SetDestination(account);
            Assert.AreEqual(account.ItemsDestinationCount(), 1);

            _item2.SetDestination(account);
            Assert.AreEqual(account.ItemsDestinationCount(), 2);

            var account2 = new Account
            {
                Name = "Test Account",
                IsValid = true,

            };
            account2.setType(_accountType1);
            account2.setCategory(_accountCategory);

            Assert.AreEqual(account2.ItemsDestinationCount(), 0);
            _item.SetDestination(account2);
            Assert.AreEqual(account.ItemsDestinationCount(), 1);
            Assert.AreEqual(account2.ItemsDestinationCount(), 1);
        }

        [TestMethod]
        public void List_of_types_updated_correctly()
        {
            AccountType accountType1 = new AccountType
            {
                Name = "Asset",
                IsDestination = true,
                IsSource = true,
                IsValid = true,
            };
            AccountType accountType2 = new AccountType
            {
                Name = "Expense",
                IsDestination = true,
                IsSource = true,
                IsValid = true,
            };

            var account = new Account
            {
                IsValid = true,
                Name = "Test Account",
            };

            Assert.AreEqual(0, accountType1.AccountCount());
            Assert.AreEqual(0, accountType2.AccountCount());
            account.setType(accountType1);

            Assert.AreEqual(1, accountType1.AccountCount());
            Assert.AreEqual(0, accountType2.AccountCount());

            account.setType(accountType2);

            Assert.AreEqual(0, accountType1.AccountCount());
            Assert.AreEqual(1, accountType2.AccountCount());
        }

        [TestMethod]
        public void List_of_categories_updated_correctly()
        {
            AccountCategory accountCategory1 = new AccountCategory
            {
                Name = "Other",
                Colour = "Orange",
                IsValid = true,
            };
            AccountCategory accountCategory2 = new AccountCategory
            {
                Name = "Other2",
                Colour = "Blue",
                IsValid = true,
            };

            var account = new Account
            {
                IsValid = true,
                Name = "Test Account",
            };

            Assert.AreEqual(0, accountCategory1.AccountCount());
            Assert.AreEqual(0, accountCategory2.AccountCount());
            account.setCategory(accountCategory1);

            Assert.AreEqual(1, accountCategory1.AccountCount());
            Assert.AreEqual(0, accountCategory2.AccountCount());

            account.setCategory(accountCategory2);

            Assert.AreEqual(0, accountCategory1.AccountCount());
            Assert.AreEqual(1, accountCategory2.AccountCount());
        } 


    }
}
