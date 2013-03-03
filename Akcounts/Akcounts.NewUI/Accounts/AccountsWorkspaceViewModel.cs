using System;
using System.ComponentModel.Composition;
using System.Linq;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.Accounts
{
    [Export(typeof(IWorkspace))]
    class AccountsWorkspaceViewModel : DocumentWorkspace<AccountViewModel>
    {
        private static int count = 1;
        private readonly Func<AccountViewModel> createAccountViewModel;

        private IObservableCollection<AccountViewModel> _accounts;

        [ImportingConstructor]
        public AccountsWorkspaceViewModel(Func<AccountViewModel> accountVMFactory)
        {
           createAccountViewModel = accountVMFactory;
           CreateDummyData();
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
            var vm = createAccountViewModel();
            vm.DisplayName = "Account " + count++;

            InsertVmToAccountsInOrder(vm);
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

        private void CreateDummyData()
        {
            var random = new Random();

            _accounts = new BindableCollection<AccountViewModel>();

            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
            CreateAccountVm(random);
        }

        private void CreateAccountVm(Random random)
        {
            var vm = createAccountViewModel();
            vm.DisplayName = "Account " + random.Next(100);
            InsertVmToAccountsInOrder(vm);
        }
    }
}
