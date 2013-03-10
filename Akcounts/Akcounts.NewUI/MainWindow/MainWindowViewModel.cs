using System.Collections.Generic;
using System.ComponentModel.Composition;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.MainWindow
{
    [Export(typeof(IMainWindow))]
    public class MainWindowViewModel : Conductor<IWorkspace>.Collection.OneActive, IMainWindow
    {
        private static readonly ILog Log = LogManager.GetLog(typeof (MainWindowViewModel));

        [ImportingConstructor]
        public MainWindowViewModel([ImportMany] IEnumerable<IWorkspace> workspaces)
        {
            Items.AddRange(workspaces);
        }
    }
}
