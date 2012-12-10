using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;
using System.Windows.Input;

namespace Akcounts.UI.ViewModel
{
    public class DeleteTransactionEventArgs : EventArgs {
        public readonly Transaction Transaction;
        public DeleteTransactionEventArgs(Transaction t)
        {
            Transaction = t;
        }
    }

    public class JournalViewModel : WorkspaceViewModel////, IDataErrorInfo
    {
        private readonly Journal _journal;
        private readonly ObservableCollection<TransactionViewModel> _transactions;
        private readonly IAccountRepository _accountRepository;
        private readonly IJournalRepository _journalRepository;
        private readonly bool _isNonTemplate;

        public JournalViewModel(Journal journal, IJournalRepository journalRepository, IAccountRepository accountRepository, bool isNonTemplate = true)
        {
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");
            _accountRepository = accountRepository;

            if (journalRepository == null) throw new ArgumentNullException("journalRepository");
            _journalRepository = journalRepository;

            if (journal == null) throw new ArgumentNullException("journal");
            _journal = journal;
            _transactions = new ObservableCollection<TransactionViewModel>();
                        
            foreach (var transaction in journal.Transactions)
                AddTransactionToInternalCollection(transaction);

            _isNonTemplate = isNonTemplate;
        }

        private void AddTransactionToInternalCollection(Transaction transaction)
        {
            var tranViewModel = new TransactionViewModel(transaction, _accountRepository);
            
            tranViewModel.RequestDelete += DeleteTransaction;
            tranViewModel.TransactionModified += RefreshJournalValidity;
            _transactions.Add(tranViewModel);
            base.OnPropertyChanged("DeleteJournalVisibility");
        }

        public ObservableCollection<TransactionViewModel> Transactions
        {
            get { return _transactions; }
        }

        public DateTime JournalDate
        {
            get { return _journal.Date; }
            set
            {
                if (value == _journal.Date) return;
                _journal.Date = value;

                OnEditted();
                base.OnPropertyChanged("JournalDate");
            }
        }

        public Journal Journal
        {
            get { return _journal; }
        }

        public string JournalDescription
        {
            get { return _journal.Description; }
            set
            {
                if (value == _journal.Description) return;
                _journal.Description = value;

                OnEditted();
                base.OnPropertyChanged("JournalDescription");
            }
        }

        public bool IsEditable
        {
            get { return !_journal.IsLocked; }
        }

        public bool IsValid
        {
            get { return _journal.IsValid; }
        }

        public bool IsVerified
        {
            get { return _journal.IsLocked; }
            set { 
                _journal.IsLocked = value;
                base.OnPropertyChanged("IsEditable");
                OnEditted();
            }
        }
        
        public void DeleteTransaction(object sender, EventArgs e)
        {
            var vm = sender as TransactionViewModel;
            if (vm == null) throw new ArgumentException("DeleteTransaction() requires an TransactionViewModel as a parameter");

            _transactions.Remove(vm);
            _journal.DeleteTransaction(vm.Transaction);

            OnEditted();
            base.OnPropertyChanged("Transactions");
            base.OnPropertyChanged("DeleteJournalVisibility");
        }

        private RelayCommand _addInTransactionCommand;
        public ICommand AddInTransactionCommand
        {
            get {
                return _addInTransactionCommand ??
                       (_addInTransactionCommand = new RelayCommand(param => OnRequestAddInTransaction()));
            }
        }

        void OnRequestAddInTransaction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            AddTransactionAndSaveJournal(transaction);
        }

        private RelayCommand _addOutTransactionCommand;
        public ICommand AddOutTransactionCommand
        {
            get {
                return _addOutTransactionCommand ??
                       (_addOutTransactionCommand = new RelayCommand(param => OnRequestAddOutTransaction()));
            }
        }

        void OnRequestAddOutTransaction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            AddTransactionAndSaveJournal(transaction);
        }

        private RelayCommand _stampCommand;
        public ICommand StampCommand
        {
            get
            {
                return _stampCommand ??
                       (_stampCommand = new RelayCommand(param => OnRequestStamp()));
            }
        }

        void OnRequestStamp()
        {
            var newJournal = new Journal(_journal.Date, _journal.Description);
            foreach (var transaction in _journal.Transactions)
            {
                new Transaction(newJournal, transaction.Direction, transaction.Account, transaction.Amount, transaction.Note);
            }
            JournalDate = JournalDate.AddMonths(1);
            _journalRepository.Save(newJournal);
        }

        public Visibility StampVisibility
        {
            get
            {
                return _isNonTemplate ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private RelayCommand _deleteJournalCommand;
        public ICommand DeleteJournalCommand
        {
            get
            {
                return _deleteJournalCommand ??
                       (_deleteJournalCommand = new RelayCommand(param => OnRequestDeleteJournal()));
            }
        }

        void OnRequestDeleteJournal()
        {
            OnRequestDelete();
            CloseCommand.Execute(null);
        }

        public Visibility DeleteJournalVisibility
        {
            get
            {
                return _journal.Transactions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void AddTransactionAndSaveJournal(Transaction transaction)
        {
            AddTransactionToInternalCollection(transaction);

            OnEditted();
            base.OnPropertyChanged("Transactions");
            SetAmountsOnUneditedTransactions();
        }
        
        private void OnEditted()
        {
            if (_isNonTemplate)
            {
                _journalRepository.Save(_journal);
            }

            base.OnPropertyChanged("IsValid");
        }

        void RefreshJournalValidity(object sender, EventArgs e)
        {
            SetAmountsOnUneditedTransactions();

            OnEditted();
            base.OnPropertyChanged("IsValid");
        }

        private void SetAmountsOnUneditedTransactions()
        {
            var inTransactions = Transactions.Where(x => x.Transaction.Direction == TransactionDirection.In).ToList();
            var outTransactions = Transactions.Where(x => x.Transaction.Direction == TransactionDirection.Out).ToList();
            
            var inAmount = inTransactions.Sum(x => x.Transaction.Amount);
            var outAmount = outTransactions.Sum(x => x.Transaction.Amount);
                        
            DistributeAmountBetweenTransactions(inAmount, outTransactions);
            DistributeAmountBetweenTransactions(outAmount, inTransactions);
        }

        private void DistributeAmountBetweenTransactions(Decimal amount, IEnumerable<TransactionViewModel> transactions)
        {
            var trans = transactions.ToList();
            
            var uneditedtransactions = trans.Where(x => x.HasAmountBeenEdited == false).ToList();
            if (!uneditedtransactions.Any()) return;

            var editedtransactions = trans.Where(x => x.HasAmountBeenEdited).ToList();
            var amountAlreadyEntered = editedtransactions.Sum(x => x.Transaction.Amount);
            
            var portionOfRemainingAmount = (amount - amountAlreadyEntered)/uneditedtransactions.Count();

            foreach (var t in uneditedtransactions)
            {
                t.Transaction.Amount = portionOfRemainingAmount;
                t.RefreshAmounts();
            }
        }

        //Shameful copy from DeletableViewModel
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(param => OnRequestDelete())); }
        }

        public event EventHandler RequestDelete;

        protected void OnRequestDelete()
        {
            EventHandler handler = RequestDelete;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
