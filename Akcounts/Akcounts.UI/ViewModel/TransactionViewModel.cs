using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{
    public class TransactionViewModel : DeletableViewModel////, IDataErrorInfo
    {
        const string amountFormat = "#,##0.00###";
        readonly private Transaction _transaction;
        readonly private IAccountRepository _accountRepository;
        private bool _hasAmountBeenEdited;
        private readonly IMainWindowViewModel _mainWindow;

        public TransactionViewModel(Transaction transaction, IAccountRepository accountRepository, IMainWindowViewModel mainWindow = null)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            if (accountRepository == null) 
                throw new ArgumentNullException("accountRepository");

            _transaction = transaction;
            _accountRepository = accountRepository;
            if (_transaction.Amount != 0M) _hasAmountBeenEdited = true;
            _mainWindow = mainWindow;
        }

        public Transaction Transaction { get { return _transaction; } }

        public string[] AccountNames
        {
            get
            {
                return _accountRepository.GetAll().Select(x => x.Name).OrderBy(x=>x).ToArray();
            }
        }

        public string AccountName
        {
            get { 
                if (_transaction.Account == null) return "";
                return _transaction.Account.Name; 
            }
            set
            {
                if (value == AccountName) return;
                var account = _accountRepository.GetByName(value);
                _transaction.Account = account;
                
                OnTransactionModified();
                base.OnPropertyChanged("AccountName");
                base.OnPropertyChanged("AccountBackgroundColour");
            }
        }

        public DateTime JournalDate
        {
            get { return _transaction.Journal.Date; }
            set
            {
                if (!IsLocked)
                {
                    _transaction.Journal.Date = value;
                }
                base.OnPropertyChanged("JournalDate");
            }
        }

        public string JournalDescription
        {
            get { return _transaction.Journal.Description; }
            set
            {
                if (!IsLocked)
                {
                    if (_transaction.Journal.Description == value) return;
                    _transaction.Journal.Description = value;
                }
                base.OnPropertyChanged("JournalDescription");
            }
        }

        public string OtherAccount
        {
            get
            {
                var journalTransactions = _transaction.Journal.Transactions;
                var otherAccounts = journalTransactions
                    .Where(x => x.Account != _transaction.Account 
                           && x.Direction != _transaction.Direction).Select(x => x.Account).Distinct().ToList();

                if (otherAccounts.Count == 0) return "";
                if (otherAccounts.Count > 1) return "[Multiple Accounts]";
                
                var otherAccount = otherAccounts.Single();
                if (otherAccount == null) return "";
                return otherAccount.Name; 
            }
        }

        public string TransactionNote
        {
            get { return _transaction.Note; }
            set
            {
                if (!IsLocked)
                {
                    if (value == _transaction.Note) return;
                    _transaction.Note = value;
                }
                base.OnPropertyChanged("TransactionNote");
            }
        }

        public string AmountIn
        {
            get { return IsAmountInEnabled ? _transaction.Amount.ToString(amountFormat) : ""; }
            set
            {
                if (!IsLocked && IsAmountInEnabled)
                {
                    if (value == AmountIn) return;
                    decimal amount;
                    bool success = decimal.TryParse(value, out amount);

                    if (success)
                    {
                        _transaction.Amount = amount;
                        _hasAmountBeenEdited = true;
                        OnTransactionModified();
                    }
                }

                base.OnPropertyChanged("AmountIn");
                base.OnPropertyChanged("AmountInBackgroundColour");
            }
        }

        public string AmountOut
        {
            get { return IsAmountOutEnabled ? _transaction.Amount.ToString(amountFormat) : ""; }
            set
            {
                if (!IsLocked && IsAmountOutEnabled)
                {
                    if (value == AmountOut) return;
                    decimal amount;
                    bool success = decimal.TryParse(value, out amount);

                    if (success)
                    {
                        _transaction.Amount = amount;
                        _hasAmountBeenEdited = true;
                        OnTransactionModified();
                    }
                }

                base.OnPropertyChanged("AmountOut");
                base.OnPropertyChanged("AmountOutBackgroundColour");
            }
        }

        public bool IsVerified
        {
            get { return _transaction.IsVerified; }
            set
            {
                _transaction.IsVerified = value;
                OnTransactionModified();
            }
        }

        public bool IsLocked
        {
            get { return _transaction.Journal.IsLocked; }
            set
            {
                if (!_transaction.Journal.IsValid) return;
                if (_transaction.Journal.IsLocked == value) return;

                _transaction.Journal.IsLocked = value;
            }
        }

        private RelayCommand _showJournalCommand;
        public ICommand ShowJournalCommand
        {
            get { return _showJournalCommand ?? (_showJournalCommand = new RelayCommand(OnShowJournal)); }
        }

        void OnShowJournal(object sender)
        {
            _mainWindow.OpenExistingJournalScreen (_transaction.Journal);
        }
 
        public bool HasAmountBeenEdited { get { return _hasAmountBeenEdited; } }

        public void RefreshAmounts()
        {
            base.OnPropertyChanged("AmountIn");
            base.OnPropertyChanged("AmountInBackgroundColour");
            base.OnPropertyChanged("AmountOut");
            base.OnPropertyChanged("AmountOutBackgroundColour");
        }

        public bool IsAmountOutEnabled
        {
            get { return _transaction.Direction == TransactionDirection.Out; }
        }

        public bool IsAmountInEnabled
        {
            get { return _transaction.Direction == TransactionDirection.In; }
        }

        public bool TransactionValid
        {
            get { return _transaction.IsValid; }
        }

        public event EventHandler TransactionModified;

        protected void OnTransactionModified()
        {
            EventHandler handler = TransactionModified;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public string ToggleDirectionText
        {
            get { return _transaction.Direction == TransactionDirection.In ? "In" : "Out"; }
        }

        private RelayCommand _toggleDirectionCommand;
        public ICommand ToggleDirectionCommand
        {
            get {
                return _toggleDirectionCommand ?? (_toggleDirectionCommand = new RelayCommand(param => OnToggleDirection()));
            }
        }

        protected void OnToggleDirection()
        {
            _transaction.Direction = _transaction.Direction == TransactionDirection.In ? TransactionDirection.Out : TransactionDirection.In;

            OnTransactionModified();
            base.OnPropertyChanged("ToggleDirectionText");
            base.OnPropertyChanged("AmountOut");
            base.OnPropertyChanged("AmountIn");
            base.OnPropertyChanged("IsAmountOutEnabled");
            base.OnPropertyChanged("IsAmountInEnabled");
            base.OnPropertyChanged("AmountInBackgroundColour");
            base.OnPropertyChanged("AmountOutBackgroundColour");
        }

        public Brush AccountBackgroundColour
        {
            get {
                if (_transaction.Account == null) return new SolidColorBrush(Colors.Pink);
                return new SolidColorBrush(Colors.White); 
            }
        }
        public Brush AmountInBackgroundColour
        {
            get {
                if (_transaction.Direction == TransactionDirection.In && _transaction.Amount == 0M) return new SolidColorBrush(Colors.Pink);
                return new SolidColorBrush(Colors.White); 
            }
        }
        public Brush AmountOutBackgroundColour
        {
            get {
                if (_transaction.Direction == TransactionDirection.Out && _transaction.Amount == 0M) return new SolidColorBrush(Colors.Pink);
                return new SolidColorBrush(Colors.White); 
            }
        }
        
        //OLD CODE
        ////string IDataErrorInfo.Error
        ////{
        ////    get { return (_customer as IDataErrorInfo).Error; }
        ////}

        ////string IDataErrorInfo.this[string propertyName]
        ////{
        ////    get
        ////    {
        ////        string error = null;

        ////        if (propertyName == "CustomerType")
        ////        {
        ////            // The IsCompany property of the Customer class 
        ////            // is Boolean, so it has no concept of being in
        ////            // an "unselected" state.  The CustomerViewModel
        ////            // class handles this mapping and validation.
        ////            error = this.ValidateCustomerType();
        ////        }
        ////        else
        ////        {
        ////            error = (_customer as IDataErrorInfo)[propertyName];
        ////        }

        ////        // Dirty the commands registered with CommandManager,
        ////        // such as our Save command, so that they are queried
        ////        // to see if they can execute now.
        ////        CommandManager.InvalidateRequerySuggested();

        ////        return error;
        ////    }
        ////}

        ////string ValidateCustomerType()
        ////{
        ////    if (this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Company ||
        ////       this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Person)
        ////        return null;

        ////    return Strings.CustomerViewModel_Error_MissingCustomerType;
        ////}
    }
}
