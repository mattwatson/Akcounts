using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using NMock2;
using NUnit.Framework;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class JournalRepository_spec
    {
        private IJournalRepository _repository;

        const string TestData = @"<journals>
  <journal id=""1"" date=""2007-01-28T00:00:00"" description=""Cake"" isVerified=""false"">
    <transactions />
  </journal>
  <journal id=""2"" date=""2011-05-16T00:00:00"" description=""Cash Withdrawal"" isVerified=""true"">
    <transactions>
      <transaction direction=""1"" account=""2"" amount=""12"" note=""testnote"" isVerified=""false"" />
      <transaction direction=""2"" account=""1"" amount=""12"" note="""" isVerified=""false"" />
    </transactions>
  </journal>
  <journal id=""3"" date=""2011-05-24T00:00:00"" description=""Morrisons"" isVerified=""false"">
    <transactions>
      <transaction direction=""2"" account=""1"" amount=""25.99"" note="""" isVerified=""false"" />
      <transaction direction=""1"" account=""2"" amount=""24.99"" note="""" isVerified=""false"" />
    </transactions>
  </journal>
</journals>";

        const string TestDataWithInvalidAccount = @"<journals>
  <journal id=""2"" date=""2011-05-16T00:00:00"" description=""Cash Withdrawal"" isVerified=""false"">
    <transactions>
      <transaction direction=""1"" account=""99"" amount=""12"" note="""" isVerified=""false"" />
    </transactions>
  </journal>
</journals>";

        private Account _account1;
        private Account _account2; 

        private Mockery _mocks;
        private IAccountRepository _mockAccountRepository;

        [SetUp]
        public void CreateInitialData()
        {
            _mocks = new Mockery();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();

            _account1 = new Account(1, "Bank", AccountType.Asset);
            _account2 = new Account(2, "Food", AccountType.Expense);

            Expect.Once.On(_mockAccountRepository).Method("GetById").With(2).Will(Return.Value(_account2));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(1).Will(Return.Value(_account1));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(1).Will(Return.Value(_account1));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(2).Will(Return.Value(_account2));

            using (var sr = new StringReader(TestData))
            using (XmlReader xml = new XmlTextReader(sr))
            {
                XElement journals = XElement.Load(xml);
                _repository = new JournalRepository(journals, _mockAccountRepository);
            }
        }

        [Test]
        public void accounts_that_do_not_exist_in_the_repository_are_loaded_as_null()
        {
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(99).Will(Return.Value(null));

            _repository = null;
            using (var sr = new StringReader(TestDataWithInvalidAccount))
            using (XmlReader xml = new XmlTextReader(sr))
            {
                XElement journals = XElement.Load(xml);
                _repository = new JournalRepository(journals, _mockAccountRepository);
            }

            var journal = _repository.GetById(2);
            var transaction = journal.Transactions.First();
            Assert.IsNull(transaction.Account);
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();
            
            Assert.AreEqual(TestData, output);
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_add_new_Journal()
        {
            var date = new DateTime(2011, 10, 10);
            var j4 = new Journal(date, "TestJounal");

            new Transaction(j4, TransactionDirection.In, _account2, 12M);
            new Transaction(j4, TransactionDirection.Out, _account1, 12M);

            j4.IsLocked = true;

            _repository.Save(j4);
            var retrievedJournal = _repository.GetById(j4.Id);

            Assert.AreEqual(j4.Id, retrievedJournal.Id);
            Assert.AreEqual(j4.Date, retrievedJournal.Date);
            Assert.AreEqual(j4.Description, retrievedJournal.Description);
            Assert.AreEqual(j4.IsLocked, retrievedJournal.IsLocked);
            Assert.AreEqual(j4.Transactions, retrievedJournal.Transactions);
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}