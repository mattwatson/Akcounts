using System;
using System.Windows.Input;

namespace Akcounts.UI.Util
{
    public abstract class DeletableViewModel : ViewModelBase 
    {
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(param => OnRequestDelete())); }
        }

        public event EventHandler RequestDelete;

        protected void OnRequestDelete()
        {
            EventHandler handler = RequestDelete;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
