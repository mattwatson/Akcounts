using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.DataAccess.Repositories
{
    public class TemplateRepository : Repository<Template>, ITemplateRepository
    {
        private readonly IAccountRepository _accountRepository;

        public TemplateRepository(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public TemplateRepository(XElement templates, IAccountRepository accountRepository) : this (accountRepository)
        {
            Initialise(templates);
        }

        public TemplateRepository(string accountDataPath, IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            InitialiseRepository(accountDataPath);
        }

        protected override sealed void Initialise(XElement xElement)
        {
            var newTemplate = new Template();

            if (xElement != null)
            {
                //There can be only one
                var template = xElement.Elements().ElementAt(0);

                ParseJournalsAndAddToNewTemplate(newTemplate, template, _accountRepository);
            }

            Entities.Add(0, newTemplate);
        }

        private void ParseJournalsAndAddToNewTemplate(Template newTemplate, XElement journals, IAccountRepository accountRepository)
        {
            foreach (XElement journal in journals.Elements())
            {
                var date = (DateTime)journal.Attribute("date");
                var description = (string)journal.Attribute("description");

                var newJournal = new Journal(0, date, description);
                var transactions = journal.Element("transactions");
                if (transactions != null)
                {
                    ParseTransactionsAndAddToNewJournal(newJournal, transactions, accountRepository);
                }

                newJournal.IsLocked = false;
                newTemplate.AddJournal(newJournal);
            }
        }

        private void ParseTransactionsAndAddToNewJournal(Journal newJournal, XElement transactions, IAccountRepository accountRepository)
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
                Account account = accountAttribute == null ? null : accountRepository.GetById((int)accountAttribute);

                var transaction = new Transaction(newJournal, direction, null, amount, note);
                transaction.SetUnidirectionalAccount(account);

                if (element.Attributes().All(x => x.Name != "isVerified")) continue;

                var isVerified = (bool)element.Attribute("isVerified");
                transaction.IsVerified = isVerified;
            }
        }

        protected override string EntityNames
        {
            get { return "templates"; }
        }

        public IList<Journal> GetTemplateJournals()
        {
            var template = Entities[0];
            return template.Journals;
        }

        //TODO Get rid of this beautiful code, unless you can find a use for it ;)
        public IList<Journal> GetImaginaryJournals(DateTime startDate, DateTime endDate)
        {
            var templateJournals = new List<Journal>();

            var template = Entities[0];
            foreach (var journal in template.Journals)
            {
                //Start with the date after the journal was last used
                var date = journal.Date.AddDays(1);

                //Find the next occurrance of the template Journal that occurs after the startDate
                while (startDate > date) date = date.AddMonths(1);

                //Create all the necessary Journals until we go past the end date
                while (date <= endDate)
                {
                    var newJournal = new Journal(0, date, journal.Description);
                    foreach (var tran in journal.Transactions)
                    {
                        var newTran = new Transaction(newJournal, tran.Direction, null, tran.Amount, tran.Note);
                        newTran.SetUnidirectionalAccount(tran.Account);
                    }

                    templateJournals.Add(newJournal);
                    date = date.AddMonths(1);
                }
            }

            return templateJournals;
        }
    }
}
