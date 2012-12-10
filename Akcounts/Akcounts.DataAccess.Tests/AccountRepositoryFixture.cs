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
    public class AccountRepositoryFixture : TestFixtureBase
    {

        #region crap that comes in the file
        public AccountRepositoryFixture()
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

        private Account _account1;
        private Account _account2;
        private Account _account3;
        private Account _account4;
        private AccountType _accountType1;
        private AccountType _accountType2;
        private AccountType _accountType3;
        private AccountCategory _accountCategory;
        private AccountCategory _accountCategory2;

        protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        private void CreateInitialData()
        {
            _accountCategory = new AccountCategory { Name = "Test", Colour = "Green", IsValid = true };
            _accountCategory2 = new AccountCategory { Name = "SomethingElse", Colour = "Green", IsValid = true };

            _accountType1 = new AccountType { Name = "Asset", IsDestination = true, IsSource = true, IsValid = true };
            _accountType2 = new AccountType { Name = "SourceOnly", IsDestination = false, IsSource = true, IsValid = true };
            _accountType3 = new AccountType { Name = "DestinationOnly", IsDestination = true, IsSource = false, IsValid = true };
            _account1 = new Account
            {
                Name = "Bank Account",
                IsValid = true,
            };
            _account1.setType(_accountType1);
            _account1.setCategory(_accountCategory);

            _account2 = new Account
            {
                Name = "Savings",
                IsValid = true,
            };
            _account2.setType(_accountType1);
            _account2.setCategory(_accountCategory);

            _account3 = new Account
            {
                Name = "SourceOnly",
                IsValid = true,
            };
            _account3.setType(_accountType2);
            _account3.setCategory(_accountCategory);

            _account4 = new Account
            {
                Name = "DestinationOnly",
                IsValid = true,
            };
            _account4.setType(_accountType3);
            _account4.setCategory(_accountCategory);

            using (ISession session = SessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(_accountType1);
                session.Save(_accountType2);
                session.Save(_accountType3);
                session.Save(_accountCategory);
                session.Save(_accountCategory2);
                session.Save(_account1);
                session.Save(_account2);
                session.Save(_account3);
                session.Save(_account4);
                transaction.Commit();
            }
        }

        [TestMethod]
        public void Can_add_new_account()
        {

            var account = new Account { Name = "Credit Card", IsValid = true };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);

            IAccountRepository Arepository = new AccountRepository();

            Arepository.Add(account);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Account>(account.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(account, fromDb);
                Assert.AreEqual(account.Name, fromDb.Name);
                Assert.AreEqual(account.IsValid, fromDb.IsValid);
                Assert.AreEqual(account.Type.Name, fromDb.Type.Name);
                Assert.AreEqual(account.Category.Name, fromDb.Category.Name);

                Assert.IsTrue(IsInCollection(account, account.Category.Accounts));
                Assert.IsTrue(IsInCollection(account, account.Type.Accounts));
            }

        }

        [TestMethod]
        public void Change_account_category()
        {

            var account = new Account { Name = "Credit Card", IsValid = true };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);
            account.setCategory(_accountCategory2);

            IAccountRepository Arepository = new AccountRepository();

            Arepository.Add(account);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Account>(account.Id);

                Assert.IsTrue(IsInCollection(account, account.Category.Accounts));
                Assert.IsFalse(IsInCollection(account, _accountCategory.Accounts));

            }

        }

        [TestMethod]
        [ExpectedException(typeof(NHibernate.Exceptions.GenericADOException))]
        public void Can_not_insert_same_account_twice()
        {
            IAccountRepository repository = new AccountRepository();
            repository.Add(_account1);
        }

        [TestMethod]
        [ExpectedException(typeof(NHibernate.StaleStateException))]
        public void Can_not_update_a_non_existing_account()
        {
            var account = new Account
            {
                Name = "Test Account",
                IsValid = true,
            };
            account.setType(_accountType1);
            account.setCategory(_accountCategory);
            IAccountRepository repository = new AccountRepository();
            repository.Update(account);
        }

        [TestMethod]
        [ExpectedException(typeof(NHibernate.Exceptions.GenericADOException))]
        public void Can_not_update_into_a_duplicate()
        {
            var account = _account1;
            account.Name = "Savings";
            IAccountRepository repository = new AccountRepository();
            repository.Update(account);
        }

        [TestMethod]
        public void Can_update_existing_account()
        {
            var account = _account1;
            account.Name = "Regular Savings";
            IAccountRepository repository = new AccountRepository();
            repository.Update(account);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Account>(account.Id);
                Assert.AreEqual(account.Name, fromDb.Name);
            }
        }

        [TestMethod]
        public void Can_delete_existing_account()
        {
            var account = _account1;
            IAccountRepository repository = new AccountRepository();
            repository.Remove(account);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Account>(account.Id);
                Assert.IsNull(fromDb);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(NHibernate.StaleStateException))]
        public void Cannot_delete_account_that_does_not_exist()
        {
            var account = _account1;
            IAccountRepository repository = new AccountRepository();
            repository.Remove(account);
            repository.Remove(account);
        }

        [TestMethod]
        public void Can_get_existing_account_by_id()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetById(_account1.Id);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_account1, fromDb);
            Assert.AreEqual(_account1.Name, fromDb.Name);
            Assert.AreEqual(_account1.Type.Name, fromDb.Type.Name);
            Assert.AreEqual(_account1.IsValid, fromDb.IsValid);
        }

        [TestMethod]
        public void Can_get_existing_account_by_name()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetByName(_account2.Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_account2, fromDb);
            Assert.AreEqual(_account2.Id, fromDb.Id);
            Assert.AreEqual(_account2.Type.Name, fromDb.Type.Name);
            Assert.AreEqual(_account2.IsValid, fromDb.IsValid);
        }

        [TestMethod]
        public void Can_get_existing_account_by_type()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetByType(_accountType1);

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_account1, fromDb));
            Assert.IsTrue(IsInCollection(_account2, fromDb));
        }

        [TestMethod]
        public void Can_get_all()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetAll();

            Assert.AreEqual(4, fromDb.Count);
            Assert.IsTrue(IsInCollection(_account1, fromDb));
            Assert.IsTrue(IsInCollection(_account2, fromDb));
            Assert.IsTrue(IsInCollection(_account3, fromDb));
            Assert.IsTrue(IsInCollection(_account4, fromDb));
        }

        [TestMethod]
        public void Can_get_all_source()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetAllSource();

            Assert.AreEqual(3, fromDb.Count);
            Assert.IsTrue(IsInCollection(_account1, fromDb));
            Assert.IsTrue(IsInCollection(_account2, fromDb));
            Assert.IsTrue(IsInCollection(_account3, fromDb));
        }

        [TestMethod]
        public void Can_get_all_destination()
        {
            IAccountRepository repository = new AccountRepository();
            var fromDb = repository.GetAllDestination();

            Assert.AreEqual(3, fromDb.Count);
            Assert.IsTrue(IsInCollection(_account1, fromDb));
            Assert.IsTrue(IsInCollection(_account2, fromDb));
            Assert.IsTrue(IsInCollection(_account4, fromDb));
        }

        private bool IsInCollection(Account account, ICollection<Account> fromDb)
        {
            foreach (var item in fromDb)
                if (item.Id == account.Id)
                    return true;

            return false;
        }
    }
}
