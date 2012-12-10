using System.Collections.ObjectModel;
using System.Linq;
using System;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;
using System.Windows.Input;

namespace Akcounts.UI.ViewModel
{
    public class AccountMaintenenceViewModel : WorkspaceViewModel
    {
        ////ObservableCollection<AccountTag> _accountTags;
        readonly ObservableCollection<AccountViewModel> _accounts;

        private readonly IAccountTagRepository _accountTagRepository;
        private readonly IAccountRepository _accountRepository;
        private RelayCommand _addAccountCommand;
        
        public AccountMaintenenceViewModel(IAccountRepository accountRepository, IAccountTagRepository accountTagRepository)
        {
            _accountRepository = accountRepository;
            _accountTagRepository = accountTagRepository;

            ////_accountTags = new ObservableCollection<AccountTag>(_accountTagRepository.GetAll());
            var accounts = _accountRepository.GetAll();

            _accounts = new ObservableCollection<AccountViewModel>();
            foreach (var account in accounts.OrderBy(x => x.Name))
                AddAccountToInternalCollection(account);
        }

        private void AddAccountToInternalCollection(Account account, bool newAccount = false, bool addToEnd = true)
        {
            var accountVM = new AccountViewModel(account, _accountRepository, _accountTagRepository);
            accountVM.RequestDelete += DeleteAccount;

            if (addToEnd) 
                _accounts.Add(accountVM);
            else
                _accounts.Insert(0, accountVM);

            if (newAccount) accountVM.AccountName = "New Account";
        }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get { return _accounts; }
        }

        public void DeleteAccount(object sender, EventArgs e)
        {
            var vm = sender as AccountViewModel;
            if (vm == null) throw new ArgumentException("DeleteAccount() requires an AccountViewModel as a parameter");

            _accounts.Remove(vm);

            var idToRemove = vm.AccountId;            
            if (idToRemove != 0) {
                var accountToRemove = _accountRepository.GetById(idToRemove);
                _accountRepository.Remove(accountToRemove);
            }
            
            base.OnPropertyChanged("Accounts");
        }

        public ICommand AddAccountCommand
        {
            get { return _addAccountCommand ?? (_addAccountCommand = new RelayCommand(param => OnRequestAddAccount())); }
        }

        void OnRequestAddAccount()
        {
            var account = new Account(0, "", AccountType.Asset);
            AddAccountToInternalCollection(account, true, false);

            base.OnPropertyChanged("Accounts");
        }
    }
}
