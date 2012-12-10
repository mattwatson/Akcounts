using System;
using System.Windows.Input;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(param => OnRequestClose())); }
        }

        public event EventHandler RequestClose;
        void OnRequestClose()
        {
            if (RequestClose != null) RequestClose(this, EventArgs.Empty);
        }
    }
}