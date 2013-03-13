using System;
using System.Globalization;
using System.Linq;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.Accounts
{
    class AccountsWorkspaceViewModel : DocumentWorkspace<AccountViewModel>
    {
        private static int count = 1;
        
        private readonly IAccountRepository _accountRepository;
        private readonly IObservableCollection<AccountViewModel> _accounts = new BindableCollection<AccountViewModel>();

        public AccountsWorkspaceViewModel(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public Func<Account, AccountViewModel> CreateAccountViewModel { get; set; }

        public void InitialiseAccountViewModels()
        {
            foreach (var account in _accountRepository.GetAll())
            {
                CreateAccountVm(account);
            }
        }

        public override string Label
        {
            get { return "Accounts"; }
        }

        public override string Icon
        {
            get { return "../Accounts/Resources/Images/account48.png"; }
        }
        
        public void New()
        {
            var account = new Account(0, count++.ToString(CultureInfo.InvariantCulture), AccountType.Asset);
            var vm = CreateAccountVm(account);
            Edit(vm);
        }

        private void InsertVmToAccountsInOrder(AccountViewModel vm)
        {
            var index = _accounts.Count(x => String.CompareOrdinal(x.DisplayName, vm.DisplayName) < 0);
            _accounts.Insert(index, vm);
        }

        public IObservableCollection<AccountViewModel> Accounts
        {
            get
            {
                return _accounts;
            }
        }

        private AccountViewModel CreateAccountVm(Account account)
        {
            var vm = CreateAccountViewModel(account);
            //var vm = new AccountViewModel(_accountRepository, new AccountTagRepository());
            vm.DisplayName = account.Name;

            InsertVmToAccountsInOrder(vm);
            return vm;
        }
    }
}
