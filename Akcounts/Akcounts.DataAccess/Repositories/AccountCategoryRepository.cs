using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Akcounts.Domain;

namespace Akcounts.DataAccess
{

    public class AccountCategoryRepository : IAccountCategoryRepository
    {
        public void Add(AccountCategory accountCategory)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(accountCategory);
                transaction.Commit();
            }
        }

        public void Update(AccountCategory accountCategory)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(accountCategory);
                transaction.Commit();
            }
        }

        public void Remove(AccountCategory accountCategory)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(accountCategory);
                transaction.Commit();
            }
        }

        public AccountCategory GetById(Guid accountCategoryId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.Get<AccountCategory>(accountCategoryId);
        }

        public AccountCategory GetByName(string name)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                AccountCategory accountCategory = session
                    .CreateQuery("from AccountCategory at where at.Name =:name")
                    .SetString("name", name)
                    .UniqueResult<AccountCategory>();
                return accountCategory;
            }
        }

        public ICollection<AccountCategory> GetAll()
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<AccountCategory> accountCategories = session
                    .CreateQuery("from AccountCategory")
                    .List<AccountCategory>();
                return accountCategories;
            }


        }   
    }
}
