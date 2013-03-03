using System;
using System.ComponentModel.Composition;
using Akcounts.NewUI.Framework;
using Caliburn.Micro;

namespace Akcounts.NewUI.MainWindow
{
    [Export(typeof(IMainWindow))]
    //TODO Change the IMainWindow below into the type of thing the MainWindow can conduct - e.g. IWorkspace
    public class MainWindowViewModel : Conductor<IMainWindow>.Collection.OneActive, IMainWindow
    {
        private static readonly ILog Log = LogManager.GetLog(typeof (MainWindowViewModel));

        private string _name;
        private string _helloString;

        public string Name
        {
            get { return _name; }
            set { 
                _name = value;
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => HelloString);
            }
        }

        public string HelloString
        {
            get { return _helloString; }
            set
            {
                _helloString = value;
                NotifyOfPropertyChange(() => HelloString);
            }
        }

        public bool CanSayHello
        {
            get { return !String.IsNullOrWhiteSpace(Name); }
        }

        public void SayHello(string name)
        {
            Log.Info("Saying Hello");
            HelloString = String.Format("Hello {0}", Name);
        }
    }
}
