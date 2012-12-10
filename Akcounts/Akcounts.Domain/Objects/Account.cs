using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;

namespace Akcounts.Domain.Objects
{
    public class NameChangeEventArgs : EventArgs {
        public readonly string NewName;
        public NameChangeEventArgs(string newName)
        {
            NewName = newName;
        }
    }

    public sealed class Account : EntityIdentifiedByInt<Account>, IDomainObject
    {
        private readonly ICollection<Transaction> _transactions;
        public Account(int id, string name, AccountType type)
        {
            Id = id;
            Name = name;
            Type = type;

            IsEnabled = true;

            _transactions = new Collection<Transaction>();
        }

        internal void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        internal void RemoveTransaction(Transaction transaction)
        {
            _transactions.Remove(transaction);
        }

        public IEnumerable<Transaction> Transactions
        {
            get { return _transactions; }
        }

        public event EventHandler<NameChangeEventArgs> NameChanged;

        private string _name;
        public string Name 
        { 
            get { return _name; } 
            set {
                var args = new NameChangeEventArgs(value);
                if (NameChanged != null) NameChanged(this, args);
                _name = value;
            } 
        }
        
        public bool IsEnabled { get; set; }

        private AccountType _type;
        public AccountType Type
        {
            get { return _type; }
            set
            {
                Decimal d;
                var isInvalidAccountType = Decimal.TryParse(value.ToString(), out d);

                if (isInvalidAccountType) throw new ArgumentException("Invalid AccountType specified - ID: " + value.ToString());
                _type = value;
            }
        }

        public decimal BalanceInFuInUv
        {
            get
            {
                var balance = _transactions
                    .Where(x => x.Direction == TransactionDirection.In)
                    .Sum(x => x.Amount);
                balance -= _transactions
                    .Where(x => x.Direction == TransactionDirection.Out)
                    .Sum(x => x.Amount);

                return balance;
            }
        }

        public decimal BalanceExFuInUv
        {
            get
            {
                var balance = _transactions
                    .Where(x => x.Direction == TransactionDirection.In && x.IsNotFuture )
                    .Sum(x => x.Amount);
                balance -= _transactions
                    .Where(x => x.Direction == TransactionDirection.Out && x.IsNotFuture)
                    .Sum(x => x.Amount);

                return balance;
            }
        }

        public decimal BalanceExFuExUv
        {
            get
            {
                var balance = _transactions
                    .Where(x => x.Direction == TransactionDirection.In && x.IsVerified && x.IsNotFuture)
                    .Sum(x => x.Amount);
                balance -= _transactions
                    .Where(x => x.Direction == TransactionDirection.Out && x.IsVerified && x.IsNotFuture)
                    .Sum(x => x.Amount);

                return balance;
            }
        }

        public decimal BalanceInFuExUv
        {
            get
            {
                var balance = _transactions
                    .Where(x => x.Direction == TransactionDirection.In && x.IsVerified)
                    .Sum(x => x.Amount);
                balance -= _transactions
                    .Where(x => x.Direction == TransactionDirection.Out && x.IsVerified)
                    .Sum(x => x.Amount);

                return balance;
            }
        }

        public bool IsValid
        {
            get {
                return ValidateId()
                    && ValidateName();
            }
        }

        private bool ValidateId()
        {
            return (Id != 0);
        }

        private bool ValidateName()
        {
            return !String.IsNullOrWhiteSpace(Name);
        }

        public override string ToString()
        {
            return Name;
        }

        private readonly List<AccountTag> _tags = new List<AccountTag>();
        public ReadOnlyCollection<AccountTag> Tags
        {
            get
            {
                return _tags.AsReadOnly();
            }
        }

        public void AddTag(AccountTag tag)
        {
            _tags.Remove(tag);
            _tags.Add(tag);
        }

        public void RemoveTag(AccountTag tag)
        {
            _tags.Remove(tag);
        }

        public void ClearTags()
        {
            _tags.Clear();
        }

        public AccountTag FirstMatchingTag(IEnumerable<AccountTag> filterTags)
        {
            return _tags.FirstOrDefault(filterTags.Contains);
        }

        public XElement EmitXml()
        {
            return new XElement("account",
                    new XAttribute("id", Id),
                    new XAttribute("name", Name),
                    new XAttribute("type", (Decimal)Type),
                    new XAttribute("isEnabled", IsEnabled),
                    new XElement("tags",
                        from tag in Tags
                        select
                        new XElement("tag",
                            new XText(tag.Id.ToString(CultureInfo.InvariantCulture))
                            )
                        )
                    );
        }
/*
            ////       _breakdowns = new Dictionary<string, IDictionary<string, Decimal>>();
            
            ////foreach (var account in Accounts) 
            ////{
            ////    var months = new Dictionary<string, Decimal>();
            ////    _breakdowns.Add(account.Name,  months);
            ////    foreach (var month in Columns)
            ////    {
            ////        months.Add(month, 0);
            ////    }

            ////    foreach (var transaction in account.Transactions)
            ////    {
            ////        if (transaction.Journal.Date < MinDate || transaction.Journal.Date > MaxDate) 
            ////            continue;

            ////        string tempDate = transaction.Journal.Date.ToString("MMM-yyyy");
            ////        _breakdowns[account.Name][tempDate] += 
            ////            transaction.Direction == TransactionDirection.In ?
            ////            transaction.Amount :
            ////            0 - transaction.Amount;
            ////    }
            ////}

            ////if (DateTime.Today <= MaxDate)
            ////{
            ////    var templateJournals = _templateRepository.GetImaginaryJournals(DateTime.Today, MaxDate);
            ////    foreach (var journal in templateJournals)
            ////    {
            ////        foreach (var transaction in journal.Transactions)
            ////        {
            ////            //Should not be needed, but what the hey?
            ////            if (transaction.Journal.Date < MinDate || transaction.Journal.Date > MaxDate)
            ////                continue;

            ////            string tempDate = transaction.Journal.Date.ToString("MMM-yyyy");
            ////            _breakdowns[transaction.Account.Name][tempDate] +=
            ////                transaction.Direction == TransactionDirection.In ?
            ////                transaction.Amount :
            ////                0 - transaction.Amount;
            ////        }
            ////    }
            ////}
                
            ////var result = new StringBuilder("Account, ");
            ////foreach (var month in Columns)
            ////{
            ////    result.AppendFormat ("{0},",month);
            ////}

            ////result.AppendLine();

            ////foreach (var account in Accounts)
            ////{
            ////    result.AppendFormat("{0},", account.Name);
            ////    foreach (var month in Columns)
            ////    {
            ////        result.AppendFormat ("{0},", _breakdowns[account.Name][month].ToString(CultureInfo.InvariantCulture));
            ////    }

            ////    result.AppendLine();    
            ////}

            ////return result.ToString();
  */
        public IList<string> BalanceHistories(IList<DateTime> dateRange)
        {
            var result = new List<string>();
            var startDate = dateRange.Min();
            var endDate = dateRange.Max();

            var transactionsBeforeRange = _transactions
                .Where(x => x.Journal.Date < startDate)
                .ToList();

            var runningBalance = transactionsBeforeRange
                .Where(x => x.Direction == TransactionDirection.In)
                .Sum(x => x.Amount);
            runningBalance -= transactionsBeforeRange
                .Where(x => x.Direction == TransactionDirection.Out)
                .Sum(x => x.Amount);

            var transactionsInRange = _transactions
                .Where(x => x.Journal.Date >= startDate && x.Journal.Date <= endDate)
                .ToList();

            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var nextDate = currentDate.AddDays(1);
                var date = currentDate;
                var transactionsToday = transactionsInRange
                    .Where(x => x.Journal.Date >= date && x.Journal.Date < nextDate)
                    .ToList();

                runningBalance += transactionsToday
                    .Where(x => x.Direction == TransactionDirection.In)
                    .Sum(x => x.Amount);
                runningBalance -= transactionsToday
                    .Where(x => x.Direction == TransactionDirection.Out)
                    .Sum(x => x.Amount);

                result.Add(runningBalance.ToString("0.00"));

                currentDate = nextDate;
            }

            return result;
        }
    }
}
