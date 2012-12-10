using System;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;

namespace Akcounts.Domain.Objects
{
    public class Transaction : IDomainObject
    {
        //TODO might have gone a bit crazy with the VerifyJournal
        public Transaction(
            Journal journal,
            TransactionDirection direction,
            Account account = null,
            Decimal amount = 0M,
            string note = ""
            )
        {
            _journal = journal;
            journal.AddTransaction(this);
            Direction = direction;
            Account = account;
            Amount = amount;
            Note = note;
        }

        private readonly Journal _journal;
        public Journal Journal
        {
            get {
                VerifyJournal();
                return _journal;
            }
        }

        private TransactionDirection _direction;
        public TransactionDirection Direction
        {
            get {
                VerifyJournal();
                return _direction;
            }
            set
            {
                if (IsLocked) throw new TransactionIsLockedAndCannotBeModifiedException();
                Decimal d;
                bool IsInvalid = Decimal.TryParse(value.ToString(), out d);
                if (IsInvalid) throw new ArgumentException();
                _direction = value;
            }
        }

        public void SetUnidirectionalAccount(Account account)
        {
            _account = account;
        }

        private Account _account;
        public Account Account
        {
            get
            {
                VerifyJournal();
                return _account;
            }
            set
            {
                if (IsLocked) throw new TransactionIsLockedAndCannotBeModifiedException();
                if (_account != null) _account.RemoveTransaction(this);

                _account = value;
                if (_account != null) _account.AddTransaction(this);
            }
        }

        private Decimal _amount;
        public Decimal Amount {
            get {
                VerifyJournal();
                return _amount; 
            }
            set {
                if (IsLocked) throw new TransactionIsLockedAndCannotBeModifiedException();
                _amount = value; 
            } 
        }

        private string _note;
        public string Note {
            get {
                VerifyJournal();
                return _note; }
            set
            {
                if (IsLocked) throw new TransactionIsLockedAndCannotBeModifiedException();
                if (value == null) throw new ArgumentNullException();
                _note = value;
            }
        }

        private bool _isVerified;
        public bool IsVerified
        {
            get
            {
                VerifyJournal();
                return _isVerified;
            }
            set
            {
                if (value && IsValid == false) throw new InValidTransactionCannotBeVerifiedException();
                _isVerified = value;
            }
        }

        public bool IsNotFuture
        {
            get { return _journal.Date < DateTime.Today.AddDays(1); }
        }

        private bool IsLocked
        {
            get { return Journal.IsLocked; }
        }

        public bool IsValid
        {
            get
            {
                VerifyJournal();
                return Account != null && Amount != 0M;
            }
        }

        private void VerifyJournal()
        {
            bool ValidJournal = _journal.Transactions.Contains(this);
            if (!ValidJournal) throw new OrphanTransactionException();
        }

        public XElement EmitXml()
        {
            return new XElement("transaction",
                new XAttribute("direction", (Decimal)Direction),
                (Account == null) ? null : new XAttribute("account", Account.Id),
                new XAttribute("amount", Amount),
                new XAttribute("note", Note),
                new XAttribute("isVerified", IsVerified)
                );
        }
    }
}
