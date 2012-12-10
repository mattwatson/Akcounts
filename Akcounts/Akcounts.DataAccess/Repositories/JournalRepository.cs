using System;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.DataAccess.Repositories
{
    public class JournalRepository : Repository<Journal>, IJournalRepository
    {
        private readonly IAccountRepository _accountRepository;
        
        public JournalRepository(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public JournalRepository(XElement journals, IAccountRepository accountRepository) 
            : this(accountRepository)
        {
            _accountRepository = accountRepository;
            Initialise(journals);
        }

        public JournalRepository(string accountDataPath, IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            InitialiseRepository(accountDataPath);
        }

        protected override sealed void Initialise(XElement xElement)
        {
            var maxJournalId = 0;

            foreach (var journal in xElement.Elements())
            {
                var id = (int)journal.Attribute("id");
                var date = (DateTime)journal.Attribute("date");
                var description = (string)journal.Attribute("description");

                var newJournal = new Journal(id, date, description);
                var transactions = journal.Element("transactions");
                if (transactions != null)
                {
                    ParseTransactionsAndAddToNewJournal(newJournal, transactions, _accountRepository);
                }

                var isVerified = (bool)journal.Attribute("isVerified");
                newJournal.IsLocked = isVerified;
                Entities.Add(id, newJournal);

                if (id > maxJournalId) maxJournalId = id;
            }
        }

        private static void ParseTransactionsAndAddToNewJournal(Journal newJournal, XElement transactions, IAccountRepository accountRepository)
        {
            foreach (XElement element in transactions.Elements())
            {
                var directionAttribute = element.Attribute("direction");
                if (directionAttribute == null) continue;

                var direction = (TransactionDirection)Enum.Parse(typeof(TransactionDirection), directionAttribute.Value);

                var amount = (decimal)element.Attribute("amount");
                var xAttribute = element.Attribute("note");
                var note = xAttribute == null ? "" : xAttribute.Value;

                var accountAttribute = element.Attribute("account");
                Account account = accountAttribute == null ? null : accountRepository.GetById((int) accountAttribute);

                var transaction = new Transaction(newJournal, direction, account, amount, note);

                if (element.Attributes().All(x => x.Name != "isVerified")) continue;

                var isVerified = (bool) element.Attribute("isVerified");
                transaction.IsVerified = isVerified;
            }
        }

        protected override string EntityNames
        {
            get { return "journals"; }
        }
    }
}
