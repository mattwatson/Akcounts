using System.Collections.Generic;

namespace Akcounts.UI.ViewModel
{
    public class AccountBalanceHistoryViewModel
    {
        public string AccountName { get; set; }
        public IList<string> BalanceHistories { get; set; }
        public bool IsSelected { get; set; }
    }
}
