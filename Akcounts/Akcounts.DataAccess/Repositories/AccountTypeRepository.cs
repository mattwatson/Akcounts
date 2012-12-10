using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Akcounts.Domain;

namespace Akcounts.DataAccess
{

    public class AccountTypeRepository : IAccountTypeRepository
    {

        public void Add(AccountType accountType)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(accountType);
                transaction.Commit();
            }
        }

        public void Update(AccountType accountType)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(accountType);
                transaction.Commit();
            }
        }

        public void Remove(AccountType accountType)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(accountType);
                transaction.Commit();
            }
        }

        public AccountType GetById(Guid accountTypeId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.Get<AccountType>(accountTypeId);
        }

        public AccountType GetByName(string name)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                AccountType accountType = session
                    .CreateQuery("from AccountType at where at.Name =:name")
                    .SetString("name", name)
                    .UniqueResult<AccountType>();
                return accountType;
            }
        }

        public ICollection<AccountType> GetAll()
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<AccountType> accountTypes = session
                    .CreateQuery("from AccountType")
                    .List<AccountType>();
                return accountTypes;
            }
        }

    }
}
