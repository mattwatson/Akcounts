using System;
using System.Collections.Generic;
using Akcounts.Domain.Objects;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{
    public class SelectableAccountViewModel : ViewModelBase
    {
        private readonly Account _account;

        public SelectableAccountViewModel(Account account)
        {
            if (account == null) throw new ArgumentNullException("account");
            _account = account;
        }
        
        public string AccountName
        {
            get {
                return _account.Name;
            }
        }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        internal IList<string> BalanceHistories(IList<DateTime> dateRange)
        {
            return _account.BalanceHistories(dateRange);
        }
    }
}
