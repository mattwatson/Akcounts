using System;
using System.ComponentModel.Composition;
using Akcounts.NewUI.Framework;

namespace Akcounts.NewUI.Accounts
{
    [Export(typeof(IWorkspace))]
    class AccountsWorkspaceViewModel : DocumentWorkspace<AccountViewModel>
    {
        private static int count = 1;
        private readonly Func<AccountViewModel> createAccountViewModel;

        [ImportingConstructor]
        public AccountsWorkspaceViewModel(Func<AccountViewModel> accountVMFactory)
        {
           createAccountViewModel = accountVMFactory;
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
            Edit(vm);
        }
    }
}
