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
    public class AccountTypeRepositoryFixture : TestFixtureBase
    {

        #region crap that comes in the file
        public AccountTypeRepositoryFixture()
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
  
        private readonly AccountType[] _accountTypes = new[]
                 {
                     new AccountType {Name = "Liability", IsSource = true, IsDestination = true, IsValid = true},
                     new AccountType {Name = "Expense", IsSource = true, IsDestination = true, IsValid = true},
                     new AccountType {Name = "Income", IsSource = true, IsDestination = false, IsValid = true},
                     new AccountType {Name = "Asset", IsSource = true, IsDestination = true, IsValid = true},
                     new AccountType {Name = "AccountPayable", IsSource = true, IsDestination = true, IsValid = false},
                     new AccountType {Name = "AccountRecievable", IsSource = true, IsDestination = true, IsValid = false},
                     new AccountType {Name = "Equity", IsSource = true, IsDestination = false, IsValid = true},
                 };


        private void CreateInitialData()
        {

            using (ISession session = SessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var account in _accountTypes)
                    session.Save(account);
                transaction.Commit();
            }
        }

        protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        [TestMethod]
        public void Can_add_new_accountType()
        {
            var accountType = new AccountType { Name = "TestAccountType", IsSource = true, IsDestination = true, IsValid = true };
            IAccountTypeRepository repository = new AccountTypeRepository();
            repository.Add(accountType);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountType>(accountType.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(accountType, fromDb);
                Assert.AreEqual(accountType.Name, fromDb.Name);
                Assert.AreEqual(accountType.IsSource, fromDb.IsSource);
                Assert.AreEqual(accountType.IsDestination, fromDb.IsDestination);
                Assert.AreEqual(accountType.IsValid, fromDb.IsValid);
            }

        }

        [TestMethod]
        public void Can_update_existing_accountType()
        {
            var accountType = _accountTypes[0];
            accountType.Name = "ModifiedAccountType";
            IAccountTypeRepository repository = new AccountTypeRepository();
            repository.Update(accountType);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountType>(accountType.Id);
                Assert.AreEqual(accountType.Name, fromDb.Name);
            }
        }

        [TestMethod]
        public void Can_delete_existing_accountType()
        {
            var accountType = _accountTypes[0];
            IAccountTypeRepository repository = new AccountTypeRepository();
            repository.Remove(accountType);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountType>(accountType.Id);
                Assert.IsNull(fromDb);
            }

        }

        [TestMethod]
        public void Can_get_existing_accountType_by_id()
        {
            IAccountTypeRepository repository = new AccountTypeRepository();
            var fromDb = repository.GetById(_accountTypes[1].Id);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_accountTypes[1], fromDb);
            Assert.AreEqual(_accountTypes[1].Name, fromDb.Name);
        }

        [TestMethod]
        public void Can_get_existing_accountType_by_name()
        {
            IAccountTypeRepository repository = new AccountTypeRepository();
            var fromDb = repository.GetByName(_accountTypes[2].Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_accountTypes[2], fromDb);
            Assert.AreEqual(_accountTypes[2].Id, fromDb.Id);
        }

        [TestMethod]
        public void Can_get_all_existing_account_Types()
        {
            IAccountTypeRepository repository = new AccountTypeRepository();
            var fromDb = repository.GetAll();

            Assert.AreEqual(fromDb.Count, 7);
            Assert.IsTrue(IsInCollection(_accountTypes[0], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[1], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[2], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[3], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[4], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[5], fromDb));
            Assert.IsTrue(IsInCollection(_accountTypes[6], fromDb));
        }

        private bool IsInCollection(AccountType account, ICollection<AccountType> fromDb)
        {
            foreach (var item in fromDb)
                if (item.Id == account.Id)
                    return true;

            return false;
        }
    }
}