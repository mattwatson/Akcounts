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

        public override string EntityNames
        {
            get { return "templates"; }
        }

        public IList<Journal> GetTemplateJournals()
        {
            var template = Entities[0];
            return template.Journals;
        }
    }
}
