using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Akcounts.Domain;
using NUnit.Framework;
using NMock2;
using System.Xml.Linq;
using System.IO;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class TransactionRepository_spec
    {
        private TransactionRepository _repository;
        private Account _account1 = new Account(1, "Bank", AccountType.Asset);
        private Journal _journal1 = new Journal(1, DateTime.Today);
        private Journal _journal2 = new Journal(2, DateTime.Today);

        const string testData = @"<transactions>
  <transaction id=""2"" journal=""1"" direction=""1"" account=""1"" amount=""10.99"" note=""Stylophone &amp; Stylodrum"" />
  <transaction id=""10"" journal=""2"" direction=""1"" amount=""0"" note="""" />
</transactions>";

        private Mockery mocks;
        private IAccountRepository mockAccountRepository;
        private IJournalRepository mockJournalRepository;

        [SetUp]
        public void CreateInitialData()
        {
            mocks = new Mockery();
            mockAccountRepository = mocks.NewMock<IAccountRepository>();
            mockJournalRepository = mocks.NewMock<IJournalRepository>();

            Expect.Once.On(mockAccountRepository).Method("GetById").With(1).Will(Return.Value(_account1));
            Expect.Once.On(mockJournalRepository).Method("GetById").With(1).Will(Return.Value(_journal1));
            Expect.Once.On(mockJournalRepository).Method("Save").With(_journal1);
            Expect.Once.On(mockJournalRepository).Method("GetById").With(2).Will(Return.Value(_journal2));
            Expect.Once.On(mockJournalRepository).Method("Save").With(_journal2);

            using (StringReader stringReader = new StringReader(testData))
            using (XmlReader transactionXml = new XmlTextReader(stringReader))
            {
                XElement transactions = XElement.Load(transactionXml);
                _repository = new TransactionRepository(transactions, mockAccountRepository, mockJournalRepository);
            }
        }

        [Test]
        public void initial_parsing_loads_journals_and_accounts()
        {
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();

            Assert.AreEqual(testData, output);
        }

        //OLD CODE
        //[Test]
        //public void Can_get_existing_transaction_by_single_date()
        //{
        //    ITransactionRepository repository = new TransactionRepository();
        //    var fromDb = repository.GetByDate(_transaction1.Date);

        //    Assert.AreEqual(1, fromDb.Count);
        //    Assert.IsTrue(fromDb.Contains(_transaction1));
        //}

        //[Test]
        //public void Can_get_existing_transaction_by_date_range1()
        //{
        //    ITransactionRepository repository = new TransactionRepository();
        //    var fromDb = repository.GetByDate(_transaction1.Date.AddDays(-1D), _transaction1.Date.AddDays(1D));

        //    Assert.AreEqual(2, fromDb.Count);
        //    Assert.IsTrue(fromDb.Contains(_transaction1));
        //    Assert.IsTrue(fromDb.Contains(_transaction3));
        //}

        //[Test]
        //public void Can_get_existing_transaction_by_date_range1_reversed()
        //{
        //    ITransactionRepository repository = new TransactionRepository();
        //    var fromDb = repository.GetByDate(_transaction1.Date.AddDays(1D), _transaction1.Date.AddDays(-1D));

        //    Assert.AreEqual(2, fromDb.Count);
        //    Assert.IsTrue(fromDb.Contains(_transaction1));
        //    Assert.IsTrue(fromDb.Contains(_transaction3));
        //}
    }
}