using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace Akcounts.NewUI.Accounts
{
    [Export(typeof(AccountViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]

    public class AccountViewModel : Screen
    {
        //TODO might need some logic in here
    }
}