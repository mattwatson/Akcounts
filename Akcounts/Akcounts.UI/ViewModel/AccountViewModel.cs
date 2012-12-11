using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;
using System.Windows;

namespace Akcounts.UI.ViewModel
{

    public class AccountViewModel : DeletableViewModel, IDataErrorInfo
    {
        private readonly Account _account;
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountTagRepository _accountTagRepository;

        private ICollection<TransactionViewModel> _allTransactions;
        private readonly ObservableCollection<TransactionViewModel> _visibleTransactions;
        private readonly ObservableCollection<AccountTagViewModel> _accountTags;

        private readonly IMainWindowViewModel _mainWindow;

        public AccountViewModel(Account account, IAccountRepository accountRepository, IAccountTagRepository accountTagRepository, IMainWindowViewModel mainWindow = null)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");
            if (accountTagRepository == null) throw new ArgumentNullException("accountTagRepository");

            _mainWindow = mainWindow;
            _account = account;
            _accountName = account.Name;
            _accountRepository = accountRepository;
            _accountTagRepository = accountTagRepository;

            _allTransactions = new Collection<TransactionViewModel>();
            _visibleTransactions = new ObservableCollection<TransactionViewModel>();
            
            _accountTags = new ObservableCollection<AccountTagViewModel>();
            foreach (var tag in account.Tags)
            {
                var tagVM = new AccountTagViewModel(tag, _accountTagRepository);
                tagVM.RequestDelete += RemoveAccountTag;
                _accountTags.Add(tagVM);
            }
        }

        private void RefreshAccountViewModels()
        {
            var allTransactions = new Collection<TransactionViewModel>();
            foreach (var transaction in _account.Transactions.OrderBy(x => x.Journal.Date))
            {
                Transaction transaction1 = transaction;
                var tranViewModel = _allTransactions.SingleOrDefault(x => x.Transaction == transaction1);

                if (tranViewModel == null)
                {
                    tranViewModel = new TransactionViewModel(transaction, _accountRepository, _mainWindow);
                    tranViewModel.TransactionModified += RefreshTransactionVisibility;
                }
                
                allTransactions.Add(tranViewModel);
            }
            _allTransactions.Clear();
            _allTransactions = allTransactions.OrderBy(x => x.JournalDate).ToList();

            RebuildVisibleTransactionList();
        }

        private void RebuildVisibleTransactionList()
        {
            var visibleTransactions = _allTransactions.Where(TransactionShouldBeVisisble).ToList();

            var union = _visibleTransactions.Union(visibleTransactions);
            var intersection = _visibleTransactions.Intersect(visibleTransactions);

            if (!union.Except(intersection).Any()) return;

            _visibleTransactions.Clear();
            foreach (var transaction in visibleTransactions) _visibleTransactions.Add(transaction);
            base.OnPropertyChanged("AccountTransactionViewModels");
            RefreshBalances();
        }

        private void RefreshBalances()
        {
            base.OnPropertyChanged("BalanceExFuExUv");
            base.OnPropertyChanged("BalanceExFuInUv");
            base.OnPropertyChanged("BalanceInFuExUv");
            base.OnPropertyChanged("BalanceInFuInUv");
        }

        private bool TransactionShouldBeVisisble(TransactionViewModel transaction)
        {
            if (transaction.JournalDate < FromDate) return false;
            if (transaction.JournalDate > ToDate) return false;
            if (transaction.Transaction.IsVerified && !_showVerified) return false;
            if (!transaction.Transaction.IsVerified && !_showUnVerified) return false;
            if (transaction.Transaction.Direction == TransactionDirection.In && !_showIn) return false;
            if (transaction.Transaction.Direction == TransactionDirection.Out && !_showOut) return false;

            return true;
        }

        private string _accountName;
        public string AccountName
        {
            get {
                return _accountName;
            }
            set
            {
                if (value == _accountName) return;
                _editingName = true;

                _accountName = value;
              
                base.OnPropertyChanged("AccountName");
                base.OnPropertyChanged("EditingName");
                base.OnPropertyChanged("NotEditingName");
            }
        }

        public string BalanceExFuExUv
        {
            get
            {
                return _account.BalanceExFuExUv.ToString("#,##0.00###");
            }
        }

        public string BalanceExFuInUv
        {
            get
            {
                return _account.BalanceExFuInUv.ToString("#,##0.00###");
            }
        }

        public string BalanceInFuExUv
            {
            get
            {
                return _account.BalanceInFuExUv.ToString("#,##0.00###");
            }
        }

        public string BalanceInFuInUv
        {
            get
            {
                return _account.BalanceInFuInUv.ToString("#,##0.00###");
            }
        }

        public ObservableCollection<TransactionViewModel> AccountTransactionViewModels
        {
            get
            {
                RefreshAccountViewModels();
                RebuildVisibleTransactionList();
                return _visibleTransactions;
            }
        }

        public static string[] AccountTypeNames
        {
            get
            {
                return Enum.GetNames(typeof(AccountType));
            }
        }

        private bool _showVerified = true;
        public bool ShowVerified
        {
            get { return _showVerified; }
            set {
                if (value == _showVerified) return;
                _showVerified = value;
                RebuildVisibleTransactionList();
            }
        }

        private bool _showUnVerified = true;
        public bool ShowUnVerified
        {
            get { return _showUnVerified; }
            set
            {
                if (value == _showUnVerified) return;
                _showUnVerified = value;
                RebuildVisibleTransactionList();
            }
        }

        private bool _showIn = true;
        public bool ShowIn
        {
            get { return _showIn; }
            set
            {
                if (value == _showIn) return;
                _showIn = value;
                RebuildVisibleTransactionList();
            }
        }

        private bool _showOut = true;
        public bool ShowOut
        {
            get { return _showOut; }
            set
            {
                if (value == _showOut) return;
                _showOut = value;
                RebuildVisibleTransactionList();
            }
        }

        private DateTime _fromDate;
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (value == _fromDate) return;
                _fromDate = value;
                RebuildVisibleTransactionList();
            }
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (value == _toDate) return;
                _toDate = value;
                RebuildVisibleTransactionList();
            }
        }

        void RefreshTransactionVisibility(object sender, EventArgs e)
        {
            var vm = sender as TransactionViewModel;
            if (vm != null)
            {
                if (_visibleTransactions.Contains(vm))
                {
                    if ((vm.Transaction.IsVerified && !_showVerified)
                        || (!vm.Transaction.IsVerified && !_showUnVerified)
                        || (vm.Transaction.Direction == TransactionDirection.In && !_showIn)
                        || (vm.Transaction.Direction == TransactionDirection.Out && !_showOut))
                    {
                        _visibleTransactions.Remove(vm);
                    }
                }
                else
                {
                    if (((vm.Transaction.IsVerified && _showVerified) || (!vm.Transaction.IsVerified && _showUnVerified))
                        && ((vm.Transaction.Direction == TransactionDirection.In && _showIn) || (vm.Transaction.Direction == TransactionDirection.Out && !_showOut)))
                    {
                        _visibleTransactions.Add(vm);
                    }
                }
            }
            else
            {
                RebuildVisibleTransactionList();
            }
        }

        public string AccountTypeName
        {
            get { return _account.Type.ToString(); }
            set {
                if (value == _account.Type.ToString()) return;

                AccountType type;
                if (TryParseAccountType(value, out type))
                {
                    _account.Type = type;

                    _accountRepository.Save(_account);
                    base.OnPropertyChanged("AccountTypeName");
                }
            }
        }

        private static bool TryParseAccountType(string value, out AccountType accountType)
        {
            var success = Enum.TryParse(value, out accountType);
            if (!success) return false;

            decimal d;
            var isInvalid = Decimal.TryParse(accountType.ToString(), out d);

            return !isInvalid;
        }

        public bool IsEnabled
        {
            get { return _account.IsEnabled; }
            set { 
                if (value == _account.IsEnabled) return;

                _account.IsEnabled = value;
                _accountRepository.Save(_account);

                base.OnPropertyChanged("IsEnabled");
                base.OnPropertyChanged("EnableButtonText");
            }
        }

        public string EnableButtonText
        {
            get { return IsEnabled ? "Disable" : "Enable"; }
        }

        public int AccountId
        {
            get { return _account.Id; }
        }

        public ObservableCollection<AccountTagViewModel> AccountTags
        {
            get { return _accountTags; }
        }

        public void RemoveAccountTag(object sender, EventArgs e)
        {
            var vm = sender as AccountTagViewModel;
            if (vm == null) throw new ArgumentException("RemoveAccountTag() requires an AccountTagViewModel as a parameter");

            var IdToRemove = vm.TagId;
            var accountTagToRemove = _account.Tags.First(x => x.Id == IdToRemove);

            _accountTags.Remove(vm);
            _account.RemoveTag(accountTagToRemove);
            _accountRepository.Save(_account);
            base.OnPropertyChanged("AccountTags");
        }

        private RelayCommand _renameCancelCommand;
        public ICommand CancelRenameCommand
        {
            get { return _renameCancelCommand ?? (_renameCancelCommand = new RelayCommand(param => OnRenameCancel())); }
        }

        void OnRenameCancel()
        {
            if (_account.Id == 0) OnRequestDelete();

            _editingName = false;
            _accountName = _account.Name;
            base.OnPropertyChanged("AccountName");
            base.OnPropertyChanged("EditingName");
            base.OnPropertyChanged("NotEditingName");
        }

        private bool _canRename;
        bool CanRename()
        {
            _canRename = _accountRepository.CouldSetAccountName(_account, _accountName);
            base.OnPropertyChanged("AccountName");

            return _canRename && !(String.IsNullOrWhiteSpace(_accountName));
        }

        private RelayCommand _renameOKCommand;
        public ICommand OKRenameCommand
        {
            get {
                return _renameOKCommand ?? (_renameOKCommand = new RelayCommand(param => OnRenameOK(), param => CanRename()));
            }
        }

        private void OnRenameOK()
        {
            _account.Name = _accountName;
            _accountRepository.Save(_account);
            _editingName = false;

            base.OnPropertyChanged("EditingName");
            base.OnPropertyChanged("NotEditingName");
        }

        private bool _editingName;
        public Visibility EditingName
        {
            get
            {
                return _editingName ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility NotEditingName
        {
            get
            {
                return _editingName ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        //Untested
        string IDataErrorInfo.Error
        {
            get {
                return null;
            }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                if (propertyName == "AccountName")
                {
                    if (String.IsNullOrWhiteSpace(AccountName)) 
                        error = "Account must have a name";
                    else if (!_canRename)
                        error = "Account has a duplicate name";
                }
                
                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }


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

        ////OLD CODE
        ////      bool _isSelected;

        /////// <summary>
        /////// Gets/sets whether this account is selected in the UI.
        /////// </summary>
        ////public bool IsSelected
        ////{
        ////    get { return _isSelected; }
        ////    set
        ////    {
        ////        if (value == _isSelected)
        ////            return;

        ////        _isSelected = value;

        ////        base.OnPropertyChanged("IsSelected");
        ////    }
        ////}
        
        ////public void Save()
        ////{
        ////    //if (!_account.IsValid)
        ////      //  throw new InvalidOperationException(Strings.AccountViewModel_Exception_CannotSave);

        ////    if (IsNewAccount)
        ////        _accountRepository.Add(_account);
            
        ////    base.OnPropertyChanged("DisplayName");
        ////}

        ////bool IsNewAccount
        ////{
        ////    get { return !_accountRepository.Contains(_account); }
        ////}

        ////bool CanSave
        ////{
        ////    get { return _account.IsValid; }
        ////}

        //#region IDataErrorInfo Members

        ////string IDataErrorInfo.Error
        ////{
        ////    get { return (_account as IDataErrorInfo).Error; }
        ////}

        ////string IDataErrorInfo.this[string propertyName]
        ////{
        ////    get
        ////    {
        ////        string error = null;

        ////        if (propertyName == "AccountType")
        ////        {
        ////            // The IsCompany property of the Account class 
        ////            // is Boolean, so it has no concept of being in
        ////            // an "unselected" state.  The AccountViewModel
        ////            // class handles this mapping and validation.
        ////            error = this.ValidateAccountType();
        ////        }
        ////        else
        ////        {
        ////            error = (_account as IDataErrorInfo)[propertyName];
        ////        }

        ////        // Dirty the commands registered with CommandManager,
        ////        // such as our Save command, so that they are queried
        ////        // to see if they can execute now.
        ////        CommandManager.InvalidateRequerySuggested();

        ////        return error;
        ////    }
        ////}

        //#endregion // IDataErrorInfo Members

    }
}
