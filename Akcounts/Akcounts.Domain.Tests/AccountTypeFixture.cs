using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akcounts.Domain;

namespace Akcounts.Domain.Tests
{
    [TestClass]
    public class AccountTypeFixture
    {

        #region crap that comes in the file
        public AccountTypeFixture()
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
        public void Can_create_new_accountType()
        {
            Account account;

            account = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };

            var accountType = new AccountType
            {
                Name = "Other",
                IsSource = true,
                IsDestination = true,
                IsValid = true,
            };

            account.setType(accountType);

            Assert.AreEqual(accountType.Id, Guid.Empty);
            Assert.AreEqual(account.Type, accountType);

        }

        [TestMethod]
        public void Account_count_returns_the_correct_result()
        {
            Account account1;
            Account account2;

            account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };

            account2 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };


            var accountType = new AccountType
            {
                Name = "Other",
                IsSource = true,
                IsDestination = true,
                IsValid = true,
            };

            Assert.AreEqual(accountType.AccountCount(), 0);

            account1.setType(accountType);
            Assert.AreEqual(accountType.AccountCount(), 1);

            account2.setType(accountType);
            Assert.AreEqual(accountType.AccountCount(), 2);

            var accountType2 = new AccountType
            {
                Name = "New One",
                IsSource = true,
                IsDestination = true,
                IsValid = true,
            };

            account1.setType(accountType2);

            Assert.AreEqual(accountType.AccountCount(), 1);
            Assert.AreEqual(accountType2.AccountCount(), 1);
        }
    }
}
