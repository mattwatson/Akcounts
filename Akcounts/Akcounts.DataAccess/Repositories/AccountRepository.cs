using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.DataAccess.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        private readonly IAccountTagRepository _accountTagRepository;

        public AccountRepository(IAccountTagRepository accountTagRepository)
        {
            _accountTagRepository = accountTagRepository;
            InitialiseRepository("Data/accounts.xml");
        }

        public void Initialise(string accountDataPath, IAccountTagRepository accountTagRepository)
        {
            InitialiseRepository(accountDataPath);
        }

        protected override sealed void Initialise(XElement xElement)
        {
            var maxId = 0;
            foreach (var account in xElement.Elements())
            {
                var newAccount = ParseAccount(account);
                Add(newAccount);

                if (newAccount.Id > maxId) maxId = newAccount.Id;
            }
        }

        private Account ParseAccount(XElement account)
        {
            var id = (int) account.Attribute("id");
            var name = (string) account.Attribute("name");
            var type = (AccountType)(Decimal)account.Attribute("type");

            var newAccount = new Account(id, name, type)
            {
                IsEnabled = (bool) account.Attribute("isEnabled")
            };

            var xElement = account.Element("tags");
            if (xElement != null)
                foreach (XElement tag in xElement.Elements())
                {
                    var accountTagId = (int) tag;
                    var accountTag = _accountTagRepository.GetById (accountTagId);
                    newAccount.AddTag(accountTag);
                }

            return newAccount;
        }

        public IEnumerable<Account> GetByType(AccountType type)
        {
            return from account in Entities.Values
                   where account.Type == type
                   select account;
        }

        public Account GetByName(string name)
        {
            return Entities.Values.Where(x => x.Name == name).Select(x => x).SingleOrDefault();
        }

        protected override void Add(Account account)
        {
            if (NameAlreadyExists(account.Name)) throw new EntityAlreadyExistsException();
            account.NameChanged += ValidateNameChange;
            base.Add(account);
        }

        private bool NameAlreadyExists(string name, int? excludedId = null)
        {
            return Entities
                .Where(account => account.Value.Name == name)
                .Any(account => !excludedId.HasValue || excludedId != account.Value.Id);
        }

        private void ValidateNameChange(object sender, NameChangeEventArgs args)
        {
            var account = sender as Account;
            if (account != null && NameAlreadyExists(args.NewName, account.Id)) throw new EntityAlreadyExistsException();
        }

        public bool CouldSetAccountName(Account account, string name)
        {
            return !NameAlreadyExists(name, account.Id);
        }

        public override string EntityNames
        {
            get { return "accounts"; }
        }
    }
}
