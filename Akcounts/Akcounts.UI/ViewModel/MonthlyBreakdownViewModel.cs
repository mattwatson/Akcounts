using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{
    public class MonthlyBreakdownViewModel : WorkspaceViewModel
    {
        private readonly IAccountRepository _accountRepository;

        public MonthlyBreakdownViewModel(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            PopulateAccountList();
        }

        private IList<DateTime> _dateRange = new List<DateTime>();

        public IList<SelectableAccountViewModel> SelectableAccounts { get; set; }

        private bool _showVerified = true;
        public bool ShowVerified
        {
            get { return _showVerified; }
            set
            {
                if (value == _showVerified) return;
                _showVerified = value;
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
            }
        }

        private DateTime _fromDate = DateTime.Today.AddDays(-1);
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (value == _fromDate) return;
                _fromDate = value;
            }
        }

        private DateTime _toDate = DateTime.Today.AddDays(10);
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (value == _toDate) return;
                _toDate = value;
            }
        }

        private void PopulateAccountList()
        {
            SelectableAccounts = _accountRepository.GetAll()
                .Where(account => account.IsEnabled)
                .Where(account => account.Type == AccountType.Asset || account.Type == AccountType.Liability)
                .OrderBy(x => x.Type).ThenBy(x => x.Name)
                .Select(x=> new SelectableAccountViewModel(x))
                .ToList();
        }

        private ObservableCollection<AccountBalanceHistoryViewModel> _accountBalanceHistories;
        public ObservableCollection<AccountBalanceHistoryViewModel> AccountBalanceHistories
        {
            get
            {
                GenerateAccountBalanceHistories();
                return _accountBalanceHistories;
            }
        }

        private void GenerateAccountBalanceHistories()
        {
            UpdateDateRange();
            
            _accountBalanceHistories = new ObservableCollection<AccountBalanceHistoryViewModel>();
            var accountsToDisplay = SelectableAccounts.Where(x => x.IsSelected);

            foreach (var accountBalanceHistory in accountsToDisplay.Select(GenerateAccountBalanceHistoryForAccount))
            {
                _accountBalanceHistories.Add(accountBalanceHistory);
            }
        }

        public void UpdateDateRange()
        {
            _dateRange = DateUtil.GenerateDateTimeRange(_fromDate, _toDate);
        }

        private AccountBalanceHistoryViewModel GenerateAccountBalanceHistoryForAccount(SelectableAccountViewModel accountVm)
        {
            return new AccountBalanceHistoryViewModel
            {
                AccountName = accountVm.AccountName,
                BalanceHistories = accountVm.BalanceHistories(_dateRange),
            };
        }
    }
}
