using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;

namespace Akcounts.Domain.Objects
{
    [Serializable]
    public sealed class Journal : EntityIdentifiedByInt<Journal>, IDomainObject
    {
        public Journal(DateTime date, string description = "") : this(0, date, description) { }

        public Journal(int id, DateTime date, string description = "")
        {
            Id = id;
            Date = date;
            Description = description;
        }

        private DateTime _date;
        public DateTime Date {
            get { return _date; } 
            set {
                if (value == DateTime.MinValue) throw new ArgumentException();
                if (IsLocked) throw new VerifiedJournalCannotBeModifiedException();
                _date = value;
            } 
        }

        private string _description;
        public string Description {
            get { return _description; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (IsLocked) throw new VerifiedJournalCannotBeModifiedException();
                _description = value;
            }
        }

        private readonly List<Transaction> _transactions = new List<Transaction>();

        public ReadOnlyCollection<Transaction> Transactions
        {
            get { 
                return _transactions.AsReadOnly(); 
            }
        }

        internal void AddTransaction(Transaction transaction)
        {
            if (IsLocked) throw new VerifiedJournalCannotBeModifiedException();
            _transactions.Remove(transaction);
            _transactions.Add(transaction);
        }

        public void DeleteTransaction(Transaction transaction)
        {
            if (IsLocked) throw new VerifiedJournalCannotBeModifiedException();
            transaction.Account = null;
            _transactions.Remove(transaction);
        }

        public bool IsValid
        {
            get {

                if (_transactions.Count == 0) return false;

                var inTotal = 0M;
                var outTotal = 0M;

                foreach (var t in _transactions)
                {
                    if (!t.IsValid) return false;
                    if (t.Direction == TransactionDirection.In) inTotal += t.Amount;
                    else outTotal += t.Amount;
                }

                return Math.Abs(inTotal - outTotal) <= 0.00001M;
            }
        }

        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                if (value && IsValid == false) throw new InValidJournalCannotBeVerifiedException();

                _isLocked = value;
            }
        }

        public XElement EmitXml()
        {
            return new XElement("journal", 
                new XAttribute("id", Id),
                new XAttribute("date", Date),
                new XAttribute("description", Description),
                new XAttribute("isVerified", IsLocked),
                new XElement("transactions",
                        from transaction in Transactions
                        select transaction.EmitXml()
                        )
                );
        }
    }
}
