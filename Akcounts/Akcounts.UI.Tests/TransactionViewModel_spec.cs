using System;
using System.Linq;
using System.Windows.Media;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Tests.TestHelper;
using Akcounts.UI.ViewModel;
using NMock2;
using NUnit.Framework;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class TransactionViewModel_spec
    {
        private Mockery _mocks;
        private IAccountRepository _mockAccountRepository;
        private IAccountTagRepository _mockAccountTagRepository;
        private IMainWindowViewModel _mockMainWindowViewModel;

        private PropertyChangedCounter _changeCounter;
        private readonly Account _account = new Account(0, "HSBC Current", AccountType.Asset);
        private readonly Journal _journal = new Journal(DateTime.Today);

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();
            _mockAccountTagRepository = _mocks.NewMock<IAccountTagRepository>();
            _mockMainWindowViewModel = _mocks.NewMock<IMainWindowViewModel>();

            _changeCounter = new PropertyChangedCounter();
        }

        [Test]
        public void can_create_new_TransactionViewModel()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account, 123.45M, "100EUR");
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            Assert.AreEqual("HSBC Current", vm.AccountName);
            Assert.AreEqual("100EUR", vm.TransactionNote);
            Assert.AreEqual("", vm.AmountOut);
            Assert.AreEqual("123.45", vm.AmountIn);
            
            Assert.IsTrue(vm.TransactionValid);
        }

        [Test]
        public void exception_raised_if_you_create_new_TransactionViewModel_with_null_transaction()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionViewModel(null, _mockAccountRepository));
        }

        [Test]
        public void exception_raised_if_you_create_new_TransactionViewModel_with_null_accountRepository()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account, 123.45M, "100EUR");
            Assert.Throws<ArgumentNullException>(() => new TransactionViewModel(transaction, null));
        }

        [Test]
        public void Amount_properties_format_values_just_the_way_I_want_them_to()
        {
            var transaction1 = new Transaction(_journal, TransactionDirection.Out, _account, 12345.67899M, "100EUR");
            var transaction2 = new Transaction(_journal, TransactionDirection.In, _account, 0.095M, "100EUR");
            var vm1 = new TransactionViewModel(transaction1, _mockAccountRepository);
            var vm2 = new TransactionViewModel(transaction2, _mockAccountRepository);

            Assert.AreEqual("12,345.67899", vm1.AmountOut);
            Assert.AreEqual("0.095", vm2.AmountIn);
        }

        [Test]
        public void AccountName_is_blank_if_Account_property_is_not_set_on_transaction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            Assert.AreEqual("", vm.AccountName);
        }

        [Test]
        public void TransactionModified_is_called_if_Account_is_changed()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };
       
            Expect.Once.On(_mockAccountRepository).Method("GetByName").With("HSBC Current").Will(Return.Value(_account));

            vm.AccountName = "HSBC Current";
            Assert.IsTrue(hasTransactionModifiedBeenCalled);
        }

        [Test]
        public void TransactionModified_is_called_if_AmountIn_is_changed()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            vm.AmountIn = "1234.56";
            Assert.IsTrue(hasTransactionModifiedBeenCalled);
        }

        [Test]
        public void TransactionModified_is_called_if_AmountOut_is_changed()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            vm.AmountOut = "1234.56";
            Assert.IsTrue(hasTransactionModifiedBeenCalled);
        }

        [Test]
        public void can_set_amountIn()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountIn = "1,234.56";
            Assert.AreEqual("1,234.56", vm.AmountIn);
            Assert.AreEqual(1234.56M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountIn"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountInBackgroundColour"));
        }

        [Test]
        public void can_set_amountOut()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "1,234.56";
            Assert.AreEqual("1,234.56", vm.AmountOut);
            Assert.AreEqual(1234.56M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOut"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOutBackgroundColour"));
        }

        [Test]
        public void setting_amountIn_to_the_same_value_does_not_cause_update()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, amount: 1234.56M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountIn = "1,234.56";
            Assert.AreEqual("1,234.56", vm.AmountIn);
            Assert.AreEqual(1234.56M, transaction.Amount);
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void setting_amountOut_to_the_same_value_does_not_cause_update()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount: 1234.56M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "1,234.56";
            Assert.AreEqual("1,234.56", vm.AmountOut);
            Assert.AreEqual(1234.56M, transaction.Amount);
            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void setting_amountIn_on_transaction_with_out_TransactionDirection_has_no_affect()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "543.21";
            vm.AmountIn = "1234.56";

            Assert.AreEqual("", vm.AmountIn);
            Assert.AreEqual("543.21", vm.AmountOut);
            Assert.AreEqual(4, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void setting_amountOut_on_transaction_with_in_TransactionDirection_has_no_affect()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "1234.56";
            vm.AmountIn = "543.21";

            Assert.AreEqual("", vm.AmountOut);
            Assert.AreEqual("543.21", vm.AmountIn);
            Assert.AreEqual(4, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(4, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void cannot_set_amountIn_on_transaction_with_out_TransactionDirection()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount: 0.99M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountIn = "1234.56";
            Assert.AreEqual(0.99M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void cannot_set_amountOut_on_transaction_with_in_TransactionDirection()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, amount: 0.99M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "1234.56";
            Assert.AreEqual(0.99M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void setting_an_invalid_AmountIn_makes_amount_zero()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, amount:123.45M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;
            
            vm.AmountIn = "invalid";
            Assert.AreEqual("123.45", vm.AmountIn);
            Assert.AreEqual(123.45M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountIn"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountInBackgroundColour"));
        }

        [Test]
        public void setting_an_invalid_AmountOut_makes_amount_zero()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount:1234.56M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.AmountOut = "invalid";
            Assert.AreEqual("1,234.56", vm.AmountOut);
            Assert.AreEqual(1234.56M, transaction.Amount);
            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOut"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOutBackgroundColour"));
        }

        [Test]
        public void IsAmountInEnabled_is_true_and_IsAmountOutEnabled_is_false_when_transaction_is_In_direction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, amount: 1234.56M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            Assert.IsTrue(vm.IsAmountInEnabled);
            Assert.IsFalse(vm.IsAmountOutEnabled);
        }

        [Test]
        public void IsAmountOutEnabled_is_true_and_IsAmountInEnabled_is_false_when_transaction_is_Out_direction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount: 1234.56M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            Assert.IsTrue(vm.IsAmountOutEnabled);
            Assert.IsFalse(vm.IsAmountInEnabled);
        }

        [Test]
        public void can_set_note_on_the_transation()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount: 1234.56M, note:"George Foreman Grill");
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.TransactionNote = "George Foreman Deluxe Grille";

            Assert.AreEqual("George Foreman Deluxe Grille", vm.TransactionNote);
            Assert.AreEqual("George Foreman Deluxe Grille", transaction.Note);

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("TransactionNote"));
        }

        [Test]
        public void setting_note_to_the_same_value_does_not_cause_update()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, amount: 1234.56M, note: "George Foreman Grill");
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            vm.TransactionNote = "George Foreman Grill";

            Assert.AreEqual("George Foreman Grill", vm.TransactionNote);
            Assert.AreEqual("George Foreman Grill", transaction.Note);

            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
        }

        [Test]
        public void ToggleDirectionText_is_lt_when_TransactionDirection_is_In()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            Assert.AreEqual("In", vm.ToggleDirectionText);
        }

        [Test]
        public void ToggleDirectionText_is_gt_when_TransactionDirection_is_Out()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            Assert.AreEqual("Out", vm.ToggleDirectionText);
        }

        [TestCase(TransactionDirection.In, TransactionDirection.Out)]
        [TestCase(TransactionDirection.Out, TransactionDirection.In)]
        public void ToggleDirectionCommand_toggles_transaction_direction(TransactionDirection initialDirection, TransactionDirection toggledDirection)
        {
            var transaction = new Transaction(_journal, initialDirection);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            vm.ToggleDirectionCommand.Execute(null);
            Assert.AreEqual(toggledDirection, transaction.Direction);

            Assert.AreEqual(7, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(7, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("ToggleDirectionText"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountIn"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOut"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsAmountInEnabled"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("IsAmountOutEnabled"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountInBackgroundColour"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AmountOutBackgroundColour"));

            Assert.IsTrue(hasTransactionModifiedBeenCalled);
        }

        [Test]
        public void when_AccountName_is_set_the_transaction_looks_up_the_account_name_in_the_repository()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            Expect.Once.On(_mockAccountRepository).Method("GetByName").With("HSBC Current").Will(Return.Value(_account));

            vm.AccountName = "HSBC Current";
            Assert.AreEqual("HSBC Current", vm.AccountName);
            Assert.AreEqual(_account, transaction.Account);
            Assert.IsTrue(hasTransactionModifiedBeenCalled);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountBackgroundColour"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void when_AccountName_is_set_to_the_same_name_nothing_happens()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            vm.AccountName = "HSBC Current";
            Assert.IsFalse(hasTransactionModifiedBeenCalled);

            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void when_AccountName_is_set_to_an_nonexistent_account_name_no_change_is_made()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            bool hasTransactionModifiedBeenCalled = false;
            vm.TransactionModified += (o, e) => { hasTransactionModifiedBeenCalled = true; };

            Expect.Once.On(_mockAccountRepository).Method("GetByName").With("Nonexistent Account").Will(Return.Value(null));

            vm.AccountName = "Nonexistent Account";
            Assert.AreEqual("", vm.AccountName);
            Assert.AreEqual(null, transaction.Account);
            Assert.IsTrue(hasTransactionModifiedBeenCalled);

            Assert.AreEqual(2, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(2, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountName"));
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountBackgroundColour"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AccountNames_retrurns_all_names_from_account_repository()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            var account1 = new Account(0, "Account Name 1", AccountType.Asset);
            var account2 = new Account(0, "Account Name 2", AccountType.Liability);

            var accounts = new Account[2];
            accounts[0] = account1;
            accounts[1] = account2;

            Expect.Once.On(_mockAccountRepository).Method("GetAll").Will(Return.Value(accounts));

            var accountNames = vm.AccountNames;

            Assert.AreEqual(2, accountNames.Count());
            Assert.AreEqual(account1.Name, accountNames[0]);
            Assert.AreEqual(account2.Name, accountNames[1]);

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AccountBackgroundColour_is_white_when_AccountName_is_valid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            
            var white = new SolidColorBrush(Colors.White);
            Assert.AreEqual(white.ToString(), vm.AccountBackgroundColour.ToString());
        }

        [Test]
        public void AmountInBackgroundColour_is_white_when_AmountIn_is_valid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, null, 123.45M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
    
            var white = new SolidColorBrush(Colors.White);
            Assert.AreEqual(white.ToString(), vm.AmountInBackgroundColour.ToString());
            Assert.AreEqual(white.ToString(), vm.AmountOutBackgroundColour.ToString());
        }

        [Test]
        public void AmountOutBackgroundColour_is_white_when_AmountOut_is_valid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, null, 123.45M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            var white = new SolidColorBrush(Colors.White);
            Assert.AreEqual(white.ToString(), vm.AmountOutBackgroundColour.ToString());
            Assert.AreEqual(white.ToString(), vm.AmountInBackgroundColour.ToString());
        }

        [Test]
        public void AccountBackgroundColour_is_pink_when_AccountName_is_invalid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, null, 123.45M);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            var pink = new SolidColorBrush(Colors.Pink);
            Assert.AreEqual(pink.ToString(), vm.AccountBackgroundColour.ToString());
        }

        [Test]
        public void AmountInBackgroundColour_is_pink_when_AmountIn_is_invalid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);

            var white = new SolidColorBrush(Colors.White);
            var pink = new SolidColorBrush(Colors.Pink);
            Assert.AreEqual(pink.ToString(), vm.AmountInBackgroundColour.ToString());
            Assert.AreEqual(white.ToString(), vm.AmountOutBackgroundColour.ToString());
        }

        [Test]
        public void AmountOutBackgroundColour_is_pink_when_AmountOut_is_invalid()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, _account);
            var vm = new TransactionViewModel(transaction, _mockAccountRepository);
            
            var white = new SolidColorBrush(Colors.White);
            var pink = new SolidColorBrush(Colors.Pink);
            Assert.AreEqual(pink.ToString(), vm.AmountOutBackgroundColour.ToString());
            Assert.AreEqual(white.ToString(), vm.AmountInBackgroundColour.ToString());
        }

        [Test]
        public void Accounts_provides_an_array_of_all_available_Accounts()
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
        public void can_change_account()
        {
            var account = new Account(1, "HSBC current", AccountType.Asset);
            var vm = new AccountViewModel(account, _mockAccountRepository, _mockAccountTagRepository, _mockMainWindowViewModel);

            vm.PropertyChanged += _changeCounter.HandlePropertyChange;

            Expect.Once.On(_mockAccountRepository).Method("Save").With(account);
            vm.AccountTypeName = "Expense";

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("AccountTypeName"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //TODO Uncomment tests
        [Test]
        public void setting_the_account_to_the_same_value_does_not_trigger_an_update()
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
        public void setting_account_to_an_invalid_value_causes_no_change_to_account()
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
    }
}
