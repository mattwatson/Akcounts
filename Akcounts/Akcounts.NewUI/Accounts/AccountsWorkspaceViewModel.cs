using System.ComponentModel.Composition;
using Akcounts.NewUI.Framework;

namespace Akcounts.NewUI.Accounts
{
    [Export(typeof(IWorkspace))]
    class AccountsWorkspaceViewModel : DocumentWorkspace<AccountViewModel>
    {
        public override string Label
        {
            get { return "Accounts"; }
        }

        public override string Icon
        {
            get { return "../Accounts/Resources/Images/account48.png"; }
        }

        //TODO put 'new' logic in here like this:
        //private static int count = 1;
        //private readonly Func<OrderViewModel> createOrderViewModel;
        //
        //[ImportingConstructor]
        //public OrdersWorkspaceViewModel(Func<OrderViewModel> orderVMFactory)
        //{
        //    createOrderViewModel = orderVMFactory;
        //}
        //
        //public void New()
        //{
        //    var vm = createAccountViewModel();
        //    vm.DisplayName = "Account " + count++;
        //    vm.IsDirty = true;
        //    Edit(vm);
        //}
    }
}
