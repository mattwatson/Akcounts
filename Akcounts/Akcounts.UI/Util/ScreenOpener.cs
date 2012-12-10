using System;
using System.Windows.Input;

namespace Akcounts.UI.Util
{
    class ScreenOpener
    {
        private bool _isScreenOpen; 
        private readonly RelayCommand _openAccountMaintenanceScreen;

        public ScreenOpener(Action<EventHandler> onOpenScreenAction)
        {
            Action<object> bob = _ => 
                {
                    _isScreenOpen = true;
                    onOpenScreenAction(OnCloseMonthlyBreakdownScreen);
                };
            
            _openAccountMaintenanceScreen = new RelayCommand(bob, canExecute => CanOpenMonthlyBreakdownScreen);
        }

        public ICommand OpenCommand
        {
            get 
            {
                return _openAccountMaintenanceScreen;   
            }
        }

        private bool CanOpenMonthlyBreakdownScreen
        {
            get
            {
                return !_isScreenOpen;
            }
        }

        void OnCloseMonthlyBreakdownScreen(object sender, EventArgs e)
        {
            _isScreenOpen = false;
        }
    }


}
