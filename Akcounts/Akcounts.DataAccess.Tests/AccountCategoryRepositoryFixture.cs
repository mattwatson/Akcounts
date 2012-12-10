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
    public class AccountCategoryRepositoryFixture : TestFixtureBase
    {

        #region crap that comes in the file
        public AccountCategoryRepositoryFixture()
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
  
                private readonly AccountCategory[] _accountCategories = new[]
                 {
                     new AccountCategory {Name = "Food", Colour = "red", IsValid = true},
                     new AccountCategory {Name = "Bills", Colour = "Green", IsValid = true},
                     new AccountCategory {Name = "Transport", Colour = "blue", IsValid = true},
                     new AccountCategory {Name = "Clothes", Colour = "Purple", IsValid = true},
                 };

        private void CreateInitialData()
        {
            using (ISession session = SessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var accountCategory in _accountCategories)
                    session.Save(accountCategory);
                transaction.Commit();
            }
        }

        protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        [TestMethod]
        public void Can_add_new_accountCategory()
        {
            var accountCategory = new AccountCategory { Name = "Savings", Colour = "Yellow", IsValid = true };
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            repository.Add(accountCategory);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountCategory>(accountCategory.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(accountCategory, fromDb);
                Assert.AreEqual(accountCategory.Name, fromDb.Name);
                Assert.AreEqual(accountCategory.Colour, fromDb.Colour);
                Assert.AreEqual(accountCategory.IsValid, fromDb.IsValid);
            }

        }

        [TestMethod]
        public void Can_update_existing_accountCategory()
        {
            var accountCategory = _accountCategories[0];
            accountCategory.Name = "ModifiedAccountCategory";
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            repository.Update(accountCategory);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountCategory>(accountCategory.Id);
                Assert.AreEqual(accountCategory.Name, fromDb.Name);
            }
        }

        [TestMethod]
        public void Can_delete_existing_accountCategory()
        {
            var accountCategory = _accountCategories[0];
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            repository.Remove(accountCategory);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<AccountCategory>(accountCategory.Id);
                Assert.IsNull(fromDb);
            }

        }

        [TestMethod]
        public void Can_get_existing_accountCategory_by_id()
        {
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            var fromDb = repository.GetById(_accountCategories[1].Id);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_accountCategories[1], fromDb);
            Assert.AreEqual(_accountCategories[1].Name, fromDb.Name);
        }

        [TestMethod]
        public void Can_get_existing_accountCategory_by_name()
        {
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            var fromDb = repository.GetByName(_accountCategories[2].Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_accountCategories[2], fromDb);
            Assert.AreEqual(_accountCategories[2].Id, fromDb.Id);
        }

        [TestMethod]
        public void Can_get_all()
        {
            IAccountCategoryRepository repository = new AccountCategoryRepository();
            var fromDb = repository.GetAll();

            Assert.AreEqual(4, fromDb.Count);
            Assert.IsTrue(IsInCollection(_accountCategories[0], fromDb));
            Assert.IsTrue(IsInCollection(_accountCategories[1], fromDb));
            Assert.IsTrue(IsInCollection(_accountCategories[2], fromDb));
            Assert.IsTrue(IsInCollection(_accountCategories[3], fromDb));
        }

        private bool IsInCollection(AccountCategory account, ICollection<AccountCategory> fromDb)
        {
            foreach (var item in fromDb)
                if (item.Id == account.Id)
                    return true;

            return false;
        }
    }
}
