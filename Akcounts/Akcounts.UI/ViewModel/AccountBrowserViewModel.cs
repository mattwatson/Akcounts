using System.Collections.ObjectModel;
using System.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using System;

namespace Akcounts.UI.ViewModel
{
    public class AccountBrowserViewModel : WorkspaceViewModel
    {
        readonly ObservableCollection<AccountViewModel> _accounts;

        private readonly IAccountTagRepository _accountTagRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMainWindowViewModel _mainWindow;
        
        public AccountBrowserViewModel(IAccountRepository accountRepository, IAccountTagRepository accountTagRepository, IMainWindowViewModel mainWindow = null)
        {
            _accountRepository = accountRepository;
            _accountTagRepository = accountTagRepository;
            _mainWindow = mainWindow;
            _accounts = new ObservableCollection<AccountViewModel>();
        }

        public AccountViewModel SelectedAccount { get; set; }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get
            {
                return _accounts;
            }
        }

        public string[] AccountNames
        {
            get
            {
                return _accountRepository.GetAll().Select(x => x.Name).OrderBy(x=>x).ToArray();
            }
        }

        public string SelectedAccountName
        {
            get
            {
                if (SelectedAccount != null) return SelectedAccount.AccountName;
                return "No account Selected... bad!";
            }
            set
            {
                if (SelectedAccount != null && SelectedAccount.AccountName == value) return;

                var account = _accountRepository.GetByName(value);
                SetAccount(account);
                base.OnPropertyChanged("Accounts");
            }
        }

        private void SetAccount(Account account)
        {
            SelectedAccount = new AccountViewModel(account, _accountRepository, _accountTagRepository, _mainWindow)
            {
                ShowVerified = ShowVerified,
                ShowUnVerified = ShowUnVerified,
                ShowIn = ShowIn,
                ShowOut = ShowOut,
                FromDate = FromDate,
                ToDate = ToDate
            };

            _accounts.Clear();
            _accounts.Add(SelectedAccount);
        }

        private bool _showVerified = true;
        public bool ShowVerified
        {
            get { return _showVerified; }
            set
            {
                if (value == _showVerified) return;
                _showVerified = value;
                SelectedAccount.ShowVerified = value;
            }
        }

        private bool _showUnVerified = true;
        public bool ShowUnVerified
        {
            get { return _showUnVerified; }
            set
            {
                if (value == _showUnVerified) return;
                _showUnVerified = value;
                SelectedAccount.ShowUnVerified = value;
            }
        }

        private bool _showIn = true;
        public bool ShowIn
        {
            get { return _showIn; }
            set
            {
                if (value == _showIn) return;
                _showIn = value;
                SelectedAccount.ShowIn = value;
            }
        }

        private bool _showOut = true;
        public bool ShowOut
        {
            get { return _showOut; }
            set
            {
                if (value == _showOut) return;
                _showOut = value;
                SelectedAccount.ShowOut = value;
            }
        }

        private DateTime _fromDate = DateTime.Today.AddDays(-60);
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (value == _fromDate) return;
                _fromDate = value;
                SelectedAccount.FromDate = value;
            }
        }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (value == _toDate) return;
                _toDate = value;
                SelectedAccount.ToDate = value;
            }
        }
    }
}
