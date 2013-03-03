using System.ComponentModel.Composition;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.Accounts
{
    [Export(typeof(IWorkspace))]
    class AccountsWorkspaceViewModel : Screen, IWorkspace
    {
        public AccountsWorkspaceViewModel()
        {
            DisplayName = Label;
        }

        public string Label
        {
            get { return "Accounts"; }
        }

        public string Icon
        {
            get { return "../Accounts/Resources/Images/account48.png"; }
        }

        public string Status
        {
            get { return string.Empty; }
        }

        public void Show()
        {
            ((IConductor)Parent).ActivateItem(this);
        }
    }
}
