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
    public class ItemRepositoryFixture : TestFixtureBase
    {

        #region crap that comes in the file
        public ItemRepositoryFixture()
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
        private AccountType _accountType4;
        private AccountCategory _accountCategory;
        private Transaction _transaction;
        private Item _item1;
        private Item _item2;

        protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        private void CreateInitialData()
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
                Name = "Income",
                IsDestination = false,
                IsSource = true,
                IsValid = true,
            };

            _accountType4 = new AccountType
            {
                Name = "Fail test",
                IsDestination = true,
                IsSource = false,
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
                IsValid = true
            };
            _account1.setType(_accountType1);
            _account1.setCategory(_accountCategory);

            _account2 = new Account
            {
                Name = "Food",
                IsValid = true
            };
            _account2.setType(_accountType2);
            _account2.setCategory(_accountCategory);

            _account3 = new Account
            {
                Name = "Pay",
                IsValid = true
            };
            _account3.setType(_accountType3);
            _account3.setCategory(_accountCategory);

            _account4 = new Account
            {
                Name = "Test",
                IsValid = true,
            };
            _account4.setType(_accountType4);
            _account4.setCategory(_accountCategory);

            DateTime tempDate = new DateTime(2009, 2, 5);
            _transaction = new Transaction
            {
                Date = tempDate,
                Description = "Some payment",
                IsVerified = false,
            };
            _item1 = new Item
            {
                Value = 123.0M,
                IsVerified = true,
                Description = "Payment 1",
            };
            _item1.SetTransaction(_transaction);
            _item1.SetSource(_account1);
            _item1.SetDestination(_account2);

            _item2 = new Item
            {
                Value = 456.0M,
                IsVerified = true,
                Description = "Payment 2",
            };
            _item2.SetTransaction(_transaction);
            _item2.SetSource(_account2);
            _item2.SetDestination(_account1);

            using (ISession session = SessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(_accountType1);
                session.Save(_accountType2);
                session.Save(_accountType3);
                session.Save(_accountType4);
                session.Save(_accountCategory);
                session.Save(_account1);
                session.Save(_account2);
                session.Save(_account3);
                session.Save(_account4);
                session.Save(_transaction);
                session.Save(_item1);
                session.Save(_item2);
                transaction.Commit();
            }
        }

        [TestMethod]
        public void Can_add_new_item()
        {
            var item = new Item
            {
                Value = 0.01M,
                Description = "Add Test",
                IsVerified = true,
            };
            item.SetTransaction(_transaction);
            item.SetSource(_account1);
            item.SetDestination(_account2);

            IItemRepository repository = new ItemRepository();
            repository.Add(item);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Item>(item.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(item, fromDb);
                Assert.AreEqual(item.Value, fromDb.Value);
                Assert.AreEqual(item.Description, fromDb.Description);
                Assert.AreEqual(item.Destination.Id, fromDb.Destination.Id);
                Assert.AreEqual(item.Source.Id, fromDb.Source.Id);
                Assert.AreEqual(item.IsVerified, fromDb.IsVerified);
                Assert.AreEqual(item.TransactionId.Id, fromDb.TransactionId.Id);

                Assert.IsTrue(IsInCollection(item, item.TransactionId.Items));
                Assert.IsTrue(IsInCollection(item, item.Source.ItemsSource));
                Assert.IsTrue(IsInCollection(item, item.Destination.ItemsDestination));
            }
        }

        [TestMethod]
        public void Can_update_existing_item()
        {
            var item = _item1;
            item.Description = "Kirsch Sweets";
            item.IsVerified = false;
            IItemRepository repository = new ItemRepository();
            repository.Update(item);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Item>(item.Id);
                Assert.AreEqual(item.Description, fromDb.Description);
                Assert.AreEqual(item.IsVerified, fromDb.IsVerified);
            }
        }

        [TestMethod]
        public void Can_delete_existing_item()
        {
            var item = _item1;
            IItemRepository repository = new ItemRepository();
            repository.Remove(item);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Item>(item.Id);
                Assert.IsNull(fromDb);
            }
        }

        [TestMethod]
        public void Can_get_existing_item_by_id()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetById(_item1.Id);

            using (ISession session = SessionFactory.OpenSession())
            {
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(_item1, fromDb);
                Assert.AreEqual(_item1.Value, fromDb.Value);
                Assert.AreEqual(_item1.Description, fromDb.Description);
                Assert.AreEqual(_item1.Destination.Id, fromDb.Destination.Id);
                Assert.AreEqual(_item1.Source.Id, fromDb.Source.Id);
                Assert.AreEqual(_item1.IsVerified, fromDb.IsVerified);
                Assert.AreEqual(_item1.TransactionId.Id, fromDb.TransactionId.Id);
                Assert.AreEqual(_item1.TransactionId.Date, fromDb.TransactionId.Date);
                Assert.AreEqual(_item1.Destination.Name, fromDb.Destination.Name);
                Assert.AreEqual(_item1.Source.Name, fromDb.Source.Name);
                Assert.AreEqual(_item1.Destination.Type.Name, fromDb.Destination.Type.Name);
                Assert.AreEqual(_item1.Source.Type.Name, fromDb.Source.Type.Name);
            }
        }


        [TestMethod]
        public void Can_get_existing_items_by_transaction()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByTransaction(_transaction);

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_items_by_account()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByAccount(_account1);

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_items_by_sourceAccount()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetBySourceAccount(_account1);

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsFalse(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_items_by_destinationAccount()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDestinationAccount(_account1);

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsFalse(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_item_by_single_date()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDate(_item1.TransactionId.Date);

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));

        }

        [TestMethod]
        public void Cannot_get_non_existing_item_for_single_date()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDate(_item1.TransactionId.Date.AddDays(-1D));

            Assert.AreEqual(0, fromDb.Count);
        }

        [TestMethod]
        public void Can_get_existing_item_by_date_range()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDate(_item1.TransactionId.Date.AddDays(-1D), _item1.TransactionId.Date.AddDays(1D));

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_item_by_date_range_reversed()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDate(_item1.TransactionId.Date.AddDays(1D), _item1.TransactionId.Date.AddDays(-1D));

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }
        [TestMethod]
        public void Cannot_get_existing_item_by_date_range()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetByDate(_item1.TransactionId.Date.AddDays(1D), _item1.TransactionId.Date.AddDays(2D));

            Assert.AreEqual(0, fromDb.Count);
            Assert.IsFalse(IsInCollection(_item1, fromDb));
            Assert.IsFalse(IsInCollection(_item2, fromDb));
        }

        [TestMethod]
        public void Can_get_all()
        {
            IItemRepository repository = new ItemRepository();
            var fromDb = repository.GetAll();

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_item1, fromDb));
            Assert.IsTrue(IsInCollection(_item2, fromDb));
        }


        private bool IsInCollection(Item item, ICollection<Item> fromDb)
        {
            foreach (var colItem in fromDb)
                if (colItem.Id == item.Id)
                    return true;

            return false;
        }
  
    }
}
