using System;
using System.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Tests.TestHelper;
using Akcounts.UI.ViewModel;
using NMock2;
using NUnit.Framework;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class JournalViewModel_spec
    {
        private Mockery _mocks;
        private IAccountRepository _mockAccountRepository;
        private IJournalRepository _mockJournalRepository;

        private PropertyChangedCounter _changeCounter;
        DateTime _date;
        Journal _journal;
        JournalViewModel _vm;
        Transaction _tran1;
        Transaction _tran2;

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();
            _mockJournalRepository = _mocks.NewMock<IJournalRepository>();

            _changeCounter = new PropertyChangedCounter();
        }

        [Test]
        public void can_create_new_JournalViewModel()
        {
            SetUpInValidJournalVM();
            Assert.AreEqual(_date, _vm.JournalDate);
            Assert.AreEqual("Morrisons", _vm.JournalDescription);
        }

        [Test]
        public void exception_raised_if_you_create_new_JournalViewModel_with_null_journal()
        {
            Assert.Throws<ArgumentNullException>(() => new JournalViewModel(null, _mockJournalRepository, _mockAccountRepository));
        }

        [Test]
        public void exception_raised_if_you_create_new_JournalViewModel_with_null_accountRepository()
        {
            SetUpValidJournalVM();
            Assert.Throws<ArgumentNullException>(() => new JournalViewModel(_journal, _mockJournalRepository, null));
        }

        [Test]
        public void exception_raised_if_you_create_new_JournalViewModel_with_null_journalRepository()
        {
            SetUpValidJournalVM();
            Assert.Throws<ArgumentNullException>(() => new JournalViewModel(_journal, null, _mockAccountRepository));
        }

        [Test]
        public void accounts_can_be_accessed_by_Accounts_property()
        {
            SetUpInValidJournalVM();
            var transactions = _vm.Transactions;
            Assert.IsNotNull(transactions);
            Assert.AreEqual(2, transactions.Count);
            Assert.AreEqual("123.45", transactions[0].AmountIn);
            Assert.AreEqual("543.21", transactions[1].AmountOut);
        }

        [Test]
        public void can_change_JournalDate_and_this_causes_events_to_be_raised()
        {
            SetUpInValidJournalVM();
            var date = _vm.JournalDate.AddDays(1);
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.JournalDate = date;

            Assert.AreEqual(date, _vm.JournalDate);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("JournalDate"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void changing_JournalDate_to_same_value_does_not_cause_update()
        {
            SetUpInValidJournalVM();
            var date = _vm.JournalDate;
            _vm.JournalDate = date;

            Assert.AreEqual(date, _vm.JournalDate);
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void can_change_JournalDescription_and_this_causes_events_to_be_raised()
        {
            SetUpInValidJournalVM();
            var desc = _vm.JournalDescription + " and something else";
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.JournalDescription = desc;

            Assert.AreEqual(desc, _vm.JournalDescription);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("JournalDescription"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void changing_JournalDescription_to_same_value_does_not_cause_update()
        {
            SetUpInValidJournalVM();
            var desc = _vm.JournalDescription;
            _vm.JournalDescription = desc;

            Assert.AreEqual(desc, _vm.JournalDescription);
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void can_delete_a_Transaction_by_calling_deleteTransaction_method()
        {
            SetUpInValidJournalVM();
            var transactionVMs = _vm.Transactions;
            var tranViewModel1 = transactionVMs.ElementAt(0);
            var tranViewModel2 = transactionVMs.ElementAt(1);

            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.DeleteTransaction(tranViewModel2, null);

            Assert.AreEqual(1, transactionVMs.Count);
            Assert.IsTrue(transactionVMs.Contains(tranViewModel1));
            Assert.IsFalse(transactionVMs.Contains(tranViewModel2));

            Assert.AreEqual(1, _journal.Transactions.Count);
            Assert.IsTrue(_journal.Transactions.Contains(_tran1));
            Assert.IsFalse(_journal.Transactions.Contains(_tran2));

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Transactions"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void passing_an_object_that_is_not_an_TransactionViewModel_to_DeleteTransaction_method_causes_an_exception()
        {
            SetUpValidJournalVM();
            var accountTag = new AccountTag(1, "HSBC current");

            Assert.Throws<ArgumentException>(() => _vm.DeleteTransaction(null, null));
            Assert.Throws<ArgumentException>(() => _vm.DeleteTransaction(accountTag, null));
        }

        [Test]
        public void can_delete_Transaction_by_calling_DeleteCommand_on_the_TransactionViewModel()
        {
            SetUpInValidJournalVM();
            var transactionVMs = _vm.Transactions;
            var tranVM1 = transactionVMs.First();
            var tranVM2 = transactionVMs.Skip(1).First();
            
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);

            var args = new DeleteTransactionEventArgs(_tran1);
            tranVM1.DeleteCommand.Execute(args);

            Assert.AreEqual(1, transactionVMs.Count);
            Assert.IsFalse(transactionVMs.Contains(tranVM1));
            Assert.IsTrue(transactionVMs.Contains(tranVM2));

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Transactions"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_add_new_InTransaction_using_AddTransactionCommand()
        {
            SetUpInValidJournalVM();
            int previouscount = _vm.Transactions.Count;

            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.AddInTransactionCommand.Execute(null);

            TransactionViewModel tranVM = _vm.Transactions.Last();
            Assert.AreEqual("", tranVM.AccountName);
            Assert.AreEqual("419.76", tranVM.AmountIn);
            Assert.AreEqual("", tranVM.AmountOut);
            Assert.AreEqual("", tranVM.TransactionNote);

            Assert.AreEqual(previouscount + 1, _vm.Transactions.Count);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Transactions"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void can_add_new_OutTransaction_using_AddTransactionCommand()
        {
            SetUpInValidJournalVM();
            int previouscount = _vm.Transactions.Count;

            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.AddOutTransactionCommand.Execute(null);

            TransactionViewModel tranVM = _vm.Transactions.Last();
            Assert.AreEqual("", tranVM.AccountName);
            Assert.AreEqual("", tranVM.AmountIn);
            Assert.AreEqual("-419.76", tranVM.AmountOut);
            Assert.AreEqual("", tranVM.TransactionNote);

            Assert.AreEqual(previouscount + 1, _vm.Transactions.Count);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Transactions"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void IsEditable_is_true_when_journal_is_not_locked()
        {
            SetUpInValidJournalVM();
            Assert.IsTrue(_vm.IsEditable);
        }

        [Test]
        public void IsEditable_is_false_when_journal_is_locked()
        {
            SetUpValidJournalVM();
            var account = new Account(1, "Bank", AccountType.Asset);
            _tran1.Account = account;
            _tran2.Account = account;
            _tran2.Amount = 123.45M;
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);

            _journal.IsLocked = true;

            Assert.IsFalse(_vm.IsEditable);
        }

        [Test]
        public void IsVerified_is_false_when_journal_is_not_verified()
        {
            SetUpValidJournalVM();
            Assert.IsFalse(_vm.IsVerified);
        }

        [Test]
        public void IsVerified_is_true_when_journal_is_verified()
        {
            SetUpValidJournalVM();
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.IsVerified = true;

            Assert.IsTrue(_vm.IsVerified);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsEditable"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void verified_journal_can_be_unverified()
        {
            SetUpValidJournalVM();
            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.IsVerified = true;

            Expect.Once.On(_mockJournalRepository).Method("Save").With(_journal);
            _vm.IsVerified = false;

            Assert.IsFalse(_vm.IsVerified);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("IsEditable"));
            Assert.AreEqual(2, _changeCounter.ChangeCount("IsValid"));
        }

        [Test]
        public void IsValid_is_false_when_journal_is_new()
        {
            SetUpInValidJournalVM();
            Assert.IsFalse(_vm.IsValid);
        }

        [Test]
        public void IsValid_is_true_when_journal_has_equal_and_opposite_transactions()
        {
            SetUpValidJournalVM();
            var account = new Account(1, "Bank", AccountType.Asset);
            _tran1.Account = account;
            _tran2.Account = account;
            _tran2.Amount = 123.45M;
            Assert.IsTrue(_vm.IsValid);
        }

        private void SetUpValidJournalVM()
        {
            _date = new DateTime(2011, 1, 28);
            _journal = new Journal(_date, "Morrisons");

            var fromAccount = new Account(1, "Bank", AccountType.Asset);
            var toAccount = new Account(2, "Food", AccountType.Expense);
            _tran1 = new Transaction(_journal, TransactionDirection.In, amount: 123.45M, account: fromAccount);
            _tran2 = new Transaction(_journal, TransactionDirection.Out, amount: 123.45M, account: toAccount);

            _vm = new JournalViewModel(_journal, _mockJournalRepository, _mockAccountRepository);
            _vm.PropertyChanged += _changeCounter.HandlePropertyChange;
        }

        private void SetUpInValidJournalVM()
        {
            _date = new DateTime(2011, 1, 28);
            _journal = new Journal(_date, "Morrisons");
            _tran1 = new Transaction(_journal, TransactionDirection.In, amount: 123.45M);
            _tran2 = new Transaction(_journal, TransactionDirection.Out, amount: 543.21M);

            _vm = new JournalViewModel(_journal, _mockJournalRepository, _mockAccountRepository);
            _vm.PropertyChanged += _changeCounter.HandlePropertyChange;
        }
    }
}
