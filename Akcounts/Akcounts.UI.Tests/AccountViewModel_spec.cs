using System;
using System.Collections.Generic;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Tests.TestHelper;
using Akcounts.UI.ViewModel;
using NMock2;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.ComponentModel;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class AccountViewModel_spec
    {
        private Mockery _mocks;
        private IAccountRepository _mockAccountRepository;
        private IAccountTagRepository _mockAccountTagRepository;
        private PropertyChangedCounter _changeCounter;

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();
            _mockAccountTagRepository = _mocks.NewMock<IAccountTagRepository>();
            _changeCounter = new PropertyChangedCounter();
        }

        [Test]
        public void can_create_new_AccountViewModel()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);

            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Assert.AreEqual(1, vm.AccountId);
            Assert.AreEqual("HSBC current", vm.AccountName);
            Assert.AreEqual("Asset", vm.AccountTypeName);
            Assert.IsTrue(vm.IsEnabled);
        }

        [Test]
        public void exception_raised_if_you_create_new_AccountViewModel_with_null_account()
        {
            Assert.Throws<ArgumentNullException>(() => new AccountViewModel(null, _mockAccountRepository, _mockAccountTagRepository));
        }

        [Test]
        public void exception_raised_if_you_create_new_AccountViewModel_with_null_accountRepository()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);

            Assert.Throws<ArgumentNullException>(() => new AccountViewModel(account, null, _mockAccountTagRepository));
        }

        [Test]
        public void exception_raised_if_you_create_new_AccountViewModel_with_null_accountTagRepository()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);

            Assert.Throws<ArgumentNullException>(() => new AccountViewModel(account, _mockAccountRepository, null));
        }

        [Test]
        public void calling_OkRenameCommand_CanExecute_causes_a_check_to_see_if_that_is_a_valid_name()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "HSBC Advance").Will(Return.Value(true));
            vm.AccountName = "HSBC Advance";
            vm.OKRenameCommand.CanExecute(null);

            Assert.AreEqual(null, ((IDataErrorInfo)vm)["AccountName"]);
            Assert.AreEqual(3, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("EditingName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("NotEditingName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void changing_AccountName_to_same_name_does_not_cause_update()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;
            vm.AccountName = "HSBC current";

            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
            Assert.AreEqual(0, _changeCounter.ChangeCount("AccountName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void changing_AccountName_name_to_valid_name_causes_OkButton_to_be_enabled()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "HSBC Advance").Will(Return.Value(true));
            vm.AccountName = "HSBC Advance";

            Assert.IsTrue(vm.OKRenameCommand.CanExecute(null));

            Assert.AreEqual(3, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("EditingName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("NotEditingName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void changing_AccountName_to_duplicate_name_causes_OkButton_to_be_disabled()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "Duplicate Name").Will(Return.Value(false));
            vm.AccountName = "Duplicate Name";

            Assert.IsFalse(vm.OKRenameCommand.CanExecute(null));

            Assert.AreEqual("Account has a duplicate name", ((IDataErrorInfo)vm)["AccountName"]);
            Assert.AreEqual(3, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("EditingName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("NotEditingName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        public void changing_AccountName_to_invalid_name_causes_OkCommand_to_be_disabled(string invalidName)
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, invalidName).Will(Return.Value(true));
            vm.AccountName = invalidName;
            Assert.IsFalse(vm.OKRenameCommand.CanExecute(null));
            Assert.AreEqual("Account must have a name", ((IDataErrorInfo)vm)["AccountName"]);

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AccountTypes_provides_an_array_of_all_possible_AccountTypes()
        {
            var accountTypeNames = AccountViewModel.AccountTypeNames;

            Assert.AreEqual(7, accountTypeNames.Length);
            Assert.AreEqual(AccountType.Asset.ToString(), accountTypeNames[0]);
            Assert.AreEqual(AccountType.Liability.ToString(), accountTypeNames[1]);
            Assert.AreEqual(AccountType.Income.ToString(), accountTypeNames[2]);
            Assert.AreEqual(AccountType.Expense.ToString(), accountTypeNames[3]);
            Assert.AreEqual(AccountType.Payable.ToString(), accountTypeNames[4]);
            Assert.AreEqual(AccountType.Receivable.ToString(), accountTypeNames[5]);
            Assert.AreEqual(AccountType.Equity.ToString(), accountTypeNames[6]);
        }

        [Test]
        public void can_change_accountType()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);
            vm.AccountTypeName = "Expense";

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountTypeName"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void can_change_accountType_twice()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Exactly(2).On(_mockAccountRepository).Method("Save").With(account);
            vm.AccountTypeName = "Expense";
            vm.AccountTypeName = "Liability";

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("AccountTypeName"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void setting_the_accountType_to_the_same_value_does_not_trigger_an_update()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AccountTypeName = "Asset";

            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
            Assert.AreEqual(0, _changeCounter.ChangeCount("AccountTypeName"));

            Assert.AreEqual(AccountType.Asset, account.Type);
        }

        [Test]
        public void setting_accountType_to_an_invalid_value_causes_no_change_to_account()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AccountTypeName = "Invalid";
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
            Assert.AreEqual(0, _changeCounter.ChangeCount("AccountTypeName"));
            Assert.AreEqual(AccountType.Asset, account.Type);
        }

        [Test]
        public void EnableButtonText_is_Enable_when_account_is_disabled()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            account.IsEnabled = false;

            Assert.AreEqual("Enable", vm.EnableButtonText);
        }

        [Test]
        public void EnableButtonText_is_Disable_when_account_is_enabled()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            account.IsEnabled = true;

            Assert.AreEqual("Disable", vm.EnableButtonText);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void can_change_enabled_status_on_account_using_IsEnabled_property(bool isEnabled)
        {
            var account = new Account(1, "HSBC current", AccountType.Asset) {IsEnabled = !isEnabled};
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);
            vm.IsEnabled = isEnabled;

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsEnabled"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("EnableButtonText"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void setting_the_enabled_status_to_the_same_value_does_not_trigger_an_update(bool isEnabled)
        {
            var account = new Account(1, "HSBC current", AccountType.Asset) {IsEnabled = isEnabled};
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.IsEnabled = isEnabled;
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
            Assert.AreEqual(0, _changeCounter.ChangeCount("IsEnabled"));
            Assert.AreEqual(0, _changeCounter.ChangeCount("EnableButtonText"));
           
            Assert.AreEqual(isEnabled, account.IsEnabled);
        }

        [Test]
        public void AccountTags_property_is_empty_if_the_Account_has_no_tags()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            ICollection<AccountTagViewModel> tagVMs = vm.AccountTags;

            Assert.AreEqual(0, tagVMs.Count);
        }

        [Test]
        public void AccountTags_property_returns_a_collection_of_AccountsTagViewModels_belonging_to_the_account()
        {
            var tag1 = new AccountTag(1, "Entertainment");
            var tag2 = new AccountTag(2, "Food");
            var tag3 = new AccountTag(3, "Services");
            var account = new Account(1, "HSBC current", AccountType.Asset);
            account.AddTag(tag1);
            account.AddTag(tag2);
            account.AddTag(tag3);

            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            ObservableCollection<AccountTagViewModel> tagVMs = vm.AccountTags;
            var tagNames = tagVMs.Select(x => x.TagName).ToList();

            Assert.AreEqual(3, tagVMs.Count);
            Assert.IsTrue(tagNames.Contains("Entertainment"));
            Assert.IsTrue(tagNames.Contains("Food"));
            Assert.IsTrue(tagNames.Contains("Services"));
        }

        [Test]
        public void can_remove_an_AccountTag_from_an_Account_by_calling_RemoveAccountTag_method()
        {
            var tag1 = new AccountTag(1, "Entertainment");
            var tag2 = new AccountTag(2, "Food");
            var tag3 = new AccountTag(3, "Services");
            var account = new Account(1, "HSBC current", AccountType.Asset);
            account.AddTag(tag1);
            account.AddTag(tag2);
            account.AddTag(tag3);

            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            ObservableCollection<AccountTagViewModel> tagVMs = vm.AccountTags;
            var tagVMToRemove = tagVMs.First(x => x.TagName == "Food");
            
            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);
            vm.RemoveAccountTag(tagVMToRemove, null);

            var tagIdsInViewModel = tagVMs.Select(x => x.TagId).ToList();

            Assert.AreEqual(2, tagVMs.Count);
            Assert.IsFalse(tagIdsInViewModel.Contains(2));
            Assert.IsTrue(tagIdsInViewModel.Contains(1));
            Assert.IsTrue(tagIdsInViewModel.Contains(3));
          
            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountTags"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void passing_an_object_that_is_not_an_AccountTagViewModel_to_RemoveAccountTag_method_causes_an_exception()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Assert.Throws<ArgumentException>(() => vm.RemoveAccountTag(account, null));
        }

        [Test]
        public void can_remove_an_AccountTag_from_an_Account_by_calling_DeleteCommand_on_the_AccountTag_ViewModel()
        {
            var tag1 = new AccountTag(1, "Entertainment");
            var tag2 = new AccountTag(2, "Food");
            var tag3 = new AccountTag(3, "Services");
            var account = new Account(1, "HSBC current", AccountType.Asset);
            account.AddTag(tag1);
            account.AddTag(tag2);
            account.AddTag(tag3);

            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            ObservableCollection<AccountTagViewModel> tagVMs = vm.AccountTags;
            var tagVMToRemove = tagVMs.First(x => x.TagId == 3);
            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);

            tagVMToRemove.DeleteCommand.Execute(null);

            var tagIdsInViewModel = tagVMs.Select(x => x.TagId).ToList();

            Assert.AreEqual(2, tagVMs.Count);
            Assert.IsFalse(tagIdsInViewModel.Contains(3));
            Assert.IsTrue(tagIdsInViewModel.Contains(1));
            Assert.IsTrue(tagIdsInViewModel.Contains(2));

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountTags"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AccountId_property_returns_accounts_id()
        {
            var account = new Account(122, "Halifax", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Assert.AreEqual(122, vm.AccountId);
        }

        [Test]
        public void OKButton_Command_calls_Save_method_on_repository_and_updates_accountName()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);
            
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);

            vm.AccountName = "New Name";
            Assert.AreEqual("HSBC current", account.Name);

            vm.OKRenameCommand.Execute(null);

            Assert.AreEqual("New Name", account.Name);
            Assert.AreEqual(3, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(5, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(2, _changeCounter.ChangeCount("EditingName"));
            Assert.AreEqual(2, _changeCounter.ChangeCount("NotEditingName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelButton_Command_cancels_edit_and_does_not_call_Save_method_on_repository()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AccountName = "New Name";
            Assert.AreEqual("HSBC current", account.Name);
            Assert.AreEqual("New Name", vm.AccountName);

            vm.CancelRenameCommand.Execute(null);

            Assert.AreEqual("HSBC current", account.Name);
            Assert.AreEqual("HSBC current", vm.AccountName);
            Assert.AreEqual(3, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(6, _changeCounter.TotalChangeCount);
            Assert.AreEqual(2, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(2, _changeCounter.ChangeCount("EditingName"));
            Assert.AreEqual(2, _changeCounter.ChangeCount("NotEditingName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelButton_Command_deletes_viewmodel_if_account_does_not_exist_in_repository()
        {
            var account = new Account(0, "New Account", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            bool deleteEventRaised = false;
            vm.RequestDelete += (o, a) => deleteEventRaised = true;

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AccountName = "New Name";

            vm.CancelRenameCommand.Execute(null);

            Assert.IsTrue(deleteEventRaised);
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void EditingName_property_is_initially_hidden()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Assert.AreEqual(Visibility.Collapsed, vm.EditingName);
            Assert.AreEqual(Visibility.Visible, vm.NotEditingName);
        }

        [Test]
        public void EditingName_property_is_visible_when_AccountName_has_been_edited()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "New Name").Will(Return.Value(true));

            vm.AccountName = "New Name";
            Assert.AreEqual(Visibility.Visible, vm.EditingName);
            Assert.AreEqual(Visibility.Collapsed, vm.NotEditingName);
        }

        [Test]
        public void EditingName_property_is_hidden_after_OkRenameCommand()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "New Name").Will(Return.Value(true));
            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);

            vm.AccountName = "New Name";

            vm.OKRenameCommand.Execute(null);

            Assert.AreEqual(Visibility.Collapsed, vm.EditingName);
            Assert.AreEqual(Visibility.Visible, vm.NotEditingName);
        }

        [Test]
        public void EditingName_property_is_hidden_after_CancelRenameCommand()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository);

            Expect.Once.On(_mockAccountRepository).Method("CouldSetAccountName").With(account, "New Name").Will(Return.Value(true));

            vm.AccountName = "New Name";

            vm.CancelRenameCommand.Execute(null);

            Assert.AreEqual(Visibility.Collapsed, vm.EditingName);
            Assert.AreEqual(Visibility.Visible, vm.NotEditingName);
        }
    }
}
