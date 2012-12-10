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
    public class TransactionRepositoryFixture : TestFixtureBase
    {

        #region crap that comes in the file
        public TransactionRepositoryFixture()
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

        private Transaction _transaction1;
        private Transaction _transaction2;
        private Transaction _transaction3;

        protected override void Before_each_test()
        {
            base.Before_each_test();
            CreateInitialData();
        }

        private void CreateInitialData()
        {
            DateTime testDate = new DateTime(2009, 1, 28);
            _transaction1 = new Transaction { Date = testDate, BusinessKey = 1, Description = "Test Transaction", IsVerified = false };
            _transaction2 = new Transaction { Date = testDate.AddDays(-2), BusinessKey = 2, Description = "Test Transaction2", IsVerified = true };
            _transaction3 = new Transaction { Date = testDate.AddDays(1), BusinessKey = 3, Description = "Test Transaction3", IsVerified = true };

            using (ISession session = SessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(_transaction1);
                session.Save(_transaction2);
                session.Save(_transaction3);
                transaction.Commit();
            }
        }

        [TestMethod]
        public void Can_add_new_transaction()
        {
            DateTime testDate = new DateTime(2009, 1, 28);
            Transaction transaction = new Transaction { Date = testDate, Description = "Test Transaction2", IsVerified = true };

            ITransactionRepository repository = new TransactionRepository();
            repository.Add(transaction);


            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Transaction>(transaction.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(transaction, fromDb);
                Assert.AreEqual(transaction.Date, fromDb.Date);
                Assert.AreEqual(transaction.IsVerified, fromDb.IsVerified);
                Assert.AreEqual(transaction.Description, fromDb.Description);
                Assert.AreEqual(transaction.BusinessKey, fromDb.BusinessKey);

            }

        }

        [TestMethod]
        public void Can_update_existing_transaction()
        {
            var transaction = _transaction1;
            transaction.Description = "Something Testy";
            ITransactionRepository repository = new TransactionRepository();
            repository.Update(transaction);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Transaction>(transaction.Id);
                Assert.AreEqual(transaction.Description, fromDb.Description);
            }
        }

        [TestMethod]
        public void Can_delete_existing_transaction()
        {
            var transaction = _transaction1;
            ITransactionRepository repository = new TransactionRepository();
            repository.Remove(transaction);

            using (ISession session = SessionFactory.OpenSession())
            {
                var fromDb = session.Get<Transaction>(transaction.Id);
                Assert.IsNull(fromDb);
            }

        }

        [TestMethod]
        public void Can_get_existing_transaction_by_id()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetById(_transaction1.Id);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_transaction1, fromDb);
            Assert.AreEqual(_transaction1.Date, fromDb.Date);
            Assert.AreEqual(_transaction1.BusinessKey, fromDb.BusinessKey);
            Assert.AreEqual(_transaction1.Description, fromDb.Description);
            Assert.AreEqual(_transaction1.IsVerified, fromDb.IsVerified);

        }

        [TestMethod]
        public void Can_get_existing_transaction_by_key()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByKey(_transaction1.BusinessKey);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_transaction1, fromDb);
            Assert.AreEqual(_transaction1.Date, fromDb.Date);
            Assert.AreEqual(_transaction1.BusinessKey, fromDb.BusinessKey);
            Assert.AreEqual(_transaction1.Description, fromDb.Description);
            Assert.AreEqual(_transaction1.IsVerified, fromDb.IsVerified);

        }

        [TestMethod]
        public void Can_get_existing_transaction_by_single_date()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date);

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction1, fromDb));

        }

        [TestMethod]
        public void Cannot_get_non_existing_transaction_for_single_date()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date.AddDays(-1));

            Assert.AreEqual(0, fromDb.Count);
        }

        [TestMethod]
        public void Can_get_existing_transaction_by_date_range1()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date.AddDays(-1D), _transaction1.Date.AddDays(1D));

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction1, fromDb));
            Assert.IsTrue(IsInCollection(_transaction3, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_transaction_by_date_range2()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date.AddDays(-2D), _transaction1.Date.AddDays(-1D));

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction2, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_transaction_by_date_range1_reversed()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date.AddDays(1D), _transaction1.Date.AddDays(-1D));

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction1, fromDb));
            Assert.IsTrue(IsInCollection(_transaction3, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_transaction_by_date_range2_reversed()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByDate(_transaction1.Date.AddDays(-1D), _transaction1.Date.AddDays(-2D));

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction2, fromDb));
        }


        [TestMethod]
        public void Can_get_existing_transaction_by_isVerified()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByIsVerified(_transaction1.IsVerified);

            Assert.AreEqual(1, fromDb.Count);
            Assert.IsFalse(IsInCollection(_transaction2, fromDb));
            Assert.IsTrue(IsInCollection(_transaction1, fromDb));
        }

        [TestMethod]
        public void Can_get_existing_transaction_by_isVerified_false()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetByIsVerified(_transaction2.IsVerified);

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction2, fromDb));
            Assert.IsTrue(IsInCollection(_transaction3, fromDb));
        }

        [TestMethod]
        public void Can_get_all()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetAll();

            Assert.AreEqual(3, fromDb.Count);
            Assert.IsTrue(IsInCollection(_transaction1, fromDb));
            Assert.IsTrue(IsInCollection(_transaction2, fromDb));
            Assert.IsTrue(IsInCollection(_transaction3, fromDb));
        }

        [TestMethod]
        public void Can_get_next_Transaction_Business_Key()
        {
            ITransactionRepository repository = new TransactionRepository();
            var fromDb = repository.GetNextTransactionBusinessKey();

            DateTime testDate = new DateTime(2009, 1, 28);
            Transaction _transaction4 = new Transaction { Date = testDate.AddDays(1), BusinessKey = repository.GetNextTransactionBusinessKey(), Description = "Test Transaction4", IsVerified = true };

            repository.Add(_transaction4);
            var fromDb2 = repository.GetNextTransactionBusinessKey();

            Assert.IsNotNull(fromDb);
            Assert.IsNotNull(fromDb2);
            Assert.AreNotEqual(fromDb, fromDb2);
            Assert.AreEqual(4, fromDb);
            Assert.AreEqual(5, fromDb2);
        }

        private bool IsInCollection(Transaction transaction, ICollection<Transaction> fromDb)
        {
            foreach (var item in fromDb)
                if (item.Id == transaction.Id)
                    return true;

            return false;
        }
    }
}
