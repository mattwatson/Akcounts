using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Tests.TestHelper;
using Akcounts.UI.ViewModel;
using NMock2;
using NUnit.Framework;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class AccountMaintenenceWindowViewModel_spec
    {
        private Mockery _mocks;
        private IAccountTagRepository _mockAccountTagRepository;
        private IAccountRepository _mockAccountRepository;
        private AccountMaintenenceViewModel _accountMaintenenceViewModel;
        private PropertyChangedCounter _changeCounter;

        private readonly Account _account1 = new Account(1, "Bank", AccountType.Asset);
        private readonly Account _account2 = new Account(2, "Holiday", AccountType.Expense);

        [SetUp]
        public void SetUp()
        {
            _changeCounter = new PropertyChangedCounter();

            _mocks = new Mockery();
            _mockAccountTagRepository = _mocks.NewMock<IAccountTagRepository>();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();

            ////Expect.Once.On(_mockAccountTagRepository).Method("GetAll").Will(Return.Value(new List<AccountTag>()));

            var accountList = new List<Account> {_account1, _account2};

            Expect.Once.On(_mockAccountRepository).Method("GetAll").Will(Return.Value(accountList));

            _accountMaintenenceViewModel = new AccountMaintenenceViewModel(_mockAccountRepository, _mockAccountTagRepository);
        }

        [Test]
        public void all_data_is_loaded_from_repositories_when_MainWindowViewModel_is_created()
        {
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void accounts_can_be_accessed_by_Accounts_property()
        {
            var accounts = _accountMaintenenceViewModel.Accounts;
            Assert.IsNotNull(accounts);
            Assert.AreEqual(2, accounts.Count);
            Assert.IsTrue(accounts[0].AccountName == _account1.Name);
            Assert.IsTrue(accounts[1].AccountName == _account2.Name);
        }

        [Test]
        public void can_delete_an_Account_by_calling_deleteAccount_method()
        {
            _accountMaintenenceViewModel.PropertyChanged += _changeCounter.HandlePropertyChange;

            var accountVMs = _accountMaintenenceViewModel.Accounts;
            var accountVMToRemove = accountVMs.First(x => x.AccountId == 2);

            Expect.Once.On(_mockAccountRepository).Method("GetById").With(2).Will(Return.Value(_account2));
            Expect.Once.On(_mockAccountRepository).Method("Remove").With(_account2);
            _accountMaintenenceViewModel.DeleteAccount(accountVMToRemove, null);

            var accountIdsInViewModel = accountVMs.Select(x => x.AccountId).ToList();

            Assert.AreEqual(1, accountVMs.Count);
            Assert.IsTrue(accountIdsInViewModel.Contains(1));
            Assert.IsFalse(accountIdsInViewModel.Contains(2));

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Accounts"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void deleting_an_account_with_Id_of_0_does_not_call_delete_in_repository()
        {
            _accountMaintenenceViewModel.PropertyChanged += _changeCounter.HandlePropertyChange;

            _accountMaintenenceViewModel.AddAccountCommand.Execute(null);
            var accountVMs = _accountMaintenenceViewModel.Accounts;
            var accountVMToRemove = accountVMs.First(x => x.AccountId == 0);

            _accountMaintenenceViewModel.DeleteAccount(accountVMToRemove, null);

            var accountIdsInViewModel = accountVMs.Select(x => x.AccountId).ToList();

            Assert.AreEqual(2, accountVMs.Count);
            Assert.IsTrue(accountIdsInViewModel.Contains(1));
            Assert.IsTrue(accountIdsInViewModel.Contains(2));
            Assert.IsFalse(accountIdsInViewModel.Contains(0));

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("Accounts"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void passing_an_object_that_is_not_an_AccountViewModel_to_DeleteAccount_method_causes_an_exception()
        {
            var accountTag = new AccountTag(1, "HSBC current");

            Assert.Throws<ArgumentException>(() => _accountMaintenenceViewModel.DeleteAccount(accountTag, null));
        }

        [Test]
        public void can_delete_an_Account_by_calling_DeleteCommand_on_the_Account_ViewModel()
        {
            _accountMaintenenceViewModel.PropertyChanged += _changeCounter.HandlePropertyChange;

            var accountVMs = _accountMaintenenceViewModel.Accounts;
            var accountVMToRemove = accountVMs.First(x => x.AccountId == 1);
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(1).Will(Return.Value(_account1));
            Expect.Once.On(_mockAccountRepository).Method("Remove").With(_account1);
            
            accountVMToRemove.DeleteCommand.Execute(null);

            var accountIdsInViewModel = accountVMs.Select(x => x.AccountId).ToList();

            Assert.AreEqual(1, accountVMs.Count);
            Assert.IsFalse(accountIdsInViewModel.Contains(1));
            Assert.IsTrue(accountIdsInViewModel.Contains(2));

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Accounts"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_add_new_account_using_AddAccountCommand()
        {
            _accountMaintenenceViewModel.PropertyChanged += _changeCounter.HandlePropertyChange;
            int previouscount = _accountMaintenenceViewModel.Accounts.Count;

            _accountMaintenenceViewModel.AddAccountCommand.Execute(null);
            AccountViewModel vm = _accountMaintenenceViewModel.Accounts.First();
            Assert.AreEqual(Visibility.Visible, vm.EditingName);

            Assert.AreEqual(previouscount + 1, _accountMaintenenceViewModel.Accounts.Count);

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("Accounts"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}
