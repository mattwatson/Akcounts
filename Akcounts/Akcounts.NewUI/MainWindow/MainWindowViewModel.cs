using System.Collections.Generic;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.MainWindow
{
    public class MainWindowViewModel : Conductor<IWorkspace>.Collection.OneActive, IMainWindow
    {
        private static readonly ILog Log = LogManager.GetLog(typeof (MainWindowViewModel));

        public MainWindowViewModel(IEnumerable<IWorkspace> workspaces)
        {
            Items.AddRange(workspaces);
        }
    }
}
