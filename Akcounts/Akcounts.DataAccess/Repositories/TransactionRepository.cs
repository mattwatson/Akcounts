using Akcounts.Domain;
using System.Xml.Linq;
using System;

namespace Akcounts.DataAccess
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private int _maxId = 0;

        public TransactionRepository(XElement transactions, IAccountRepository accountRepository, IJournalRepository journalRepository)
        {
            foreach (XElement element in transactions.Elements())
            {
                var id = int.Parse(element.Attribute("id").Value);

                var journalId = (int)element.Attribute("journal");
                var direction = (TransactionDirection) Enum.Parse(typeof(TransactionDirection), element.Attribute("direction").Value);

                var amount = (decimal) element.Attribute("amount");
                var note = element.Attribute("note").Value;

                Journal journal = journalRepository.GetById(journalId);
                Account account;
                try
                {
                    var accountId = (int)element.Attribute("account");
                    account = accountRepository.GetById(accountId);
                }
                catch
                {
                    account = null;
                }

                Transaction transaction = new Transaction(id, journal, direction, account, amount, note);
                journalRepository.Save(journal);

                _entities.Add(id, transaction);
                if (id > _maxId) _maxId = id;
            }
        }

        protected override string EntityNames
        {
            get { return "transactions"; }
        }
    }
}
