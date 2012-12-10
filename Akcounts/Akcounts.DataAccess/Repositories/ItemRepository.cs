using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Akcounts.Domain;

namespace Akcounts.DataAccess
{

    public class ItemRepository : IItemRepository
    {
        public void Add(Item item)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(item);
                transaction.Commit();
            }
        }

        public void Update(Item item)
        {

            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(item);
                transaction.Commit();
            }
        }

        public void Remove(Item item)
        {

            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(item);
                transaction.Commit();
            }
        }

        public Item GetById(Guid itemId)
        {
            Item item;
            using (ISession session = NHibernateHelper.OpenSession())
            {
                item = session.Get<Item>(itemId);
                NHibernateUtil.Initialize(item.TransactionId);
                NHibernateUtil.Initialize(item.Destination);
                NHibernateUtil.Initialize(item.Source);
                NHibernateUtil.Initialize(item.Destination.Type);
                NHibernateUtil.Initialize(item.Source.Type);

            }
            return item;

        }

        public ICollection<Item> GetByAccount(Account account)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Source.Type " +
                    " inner join fetch i.Source.Category " +
                    " inner join fetch i.Destination " +
                    " inner join fetch i.Destination.Type " +
                    " inner join fetch i.Destination.Category " +
                    " where i.Source.id=:account " +
                    " or i.Destination.id=:account ")
                    .SetString("account", account.Id.ToString())
                    .List<Item>();
                return items;
            }
        }

        public ICollection<Item> GetByDestinationAccount(Account account)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Source.Type " +
                    " inner join fetch i.Source.Category " +
                    " inner join fetch i.Destination " +
                    " inner join fetch i.Destination.Type " +
                    " inner join fetch i.Destination.Category " +
                    " inner join fetch i.TransactionId " +
                    " where i.Destination.id=:account ")
                    .SetString("account", account.Id.ToString())
                    .List<Item>();
                return items;
            }
        }


        public ICollection<Item> GetBySourceAccount(Account account)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Source.Type " +
                    " inner join fetch i.Source.Category " +
                    " inner join fetch i.Destination " +
                    " inner join fetch i.Destination.Type " +
                    " inner join fetch i.Destination.Category " +
                    " inner join fetch i.TransactionId " +
                    " where i.Source.id=:account ")
                    .SetString("account", account.Id.ToString())
                    .List<Item>();
                return items;
            }
        }

        public ICollection<Item> GetByTransaction(Transaction tran)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Source.Type " +
                    " inner join fetch i.Source.Category " +
                    " inner join fetch i.Destination " +
                    " inner join fetch i.Destination.Type " +
                    " inner join fetch i.Destination.Category " +
                    " inner join fetch i.TransactionId " +
                    " where i.TransactionId.id=:tran ")
                    .SetString("tran", tran.Id.ToString())
                    .List<Item>();
                return items;
            }
        }

        public ICollection<Item> GetByDate(DateTime startDate, DateTime endDate, bool includeValid, bool includeInvalid)
        {
            if (startDate > endDate)
            {
                DateTime tempDate = endDate;
                endDate = startDate;
                startDate = tempDate;
            }

            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.TransactionId " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Destination " +
                    " where i.TransactionId.Date>=:startDate " +
                    " and i.TransactionId.Date<=:endDate " +
                    " and ((i.TransactionId.IsVerified = 1 and :includeValid = 'true') " +
                    " or (i.TransactionId.IsVerified = 0 and :includeInvalid = 'true')) " +
                    " order by i.TransactionId.Date, i.TransactionId.BusinessKey")
                    .SetString("startDate", startDate.ToString("dd-MMM-yyyy"))
                    .SetString("endDate", endDate.ToString("dd-MMM-yyyy"))
                    .SetString("includeValid", includeValid.ToString())
                    .SetString("includeInvalid", includeInvalid.ToString())
                    .List<Item>();
                return items;
            }
        }

        public ICollection<Item> GetByDate(DateTime startDate, DateTime endDate, bool includeValid)
        {
            return GetByDate(startDate, endDate, includeValid, true);
        }

        public ICollection<Item> GetByDate(DateTime startDate, DateTime endDate)
        {
            return GetByDate(startDate, endDate, true, true);
        }

        public ICollection<Item> GetByDate(DateTime getDate)
        {
            return GetByDate(getDate, getDate, true, true);
        }

        public ICollection<Item> GetAll()
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var items = session
                    .CreateQuery("from Item i " +
                    " inner join fetch i.Source " +
                    " inner join fetch i.Source.Type " +
                    " inner join fetch i.Source.Category " +
                    " inner join fetch i.Destination " +
                    " inner join fetch i.Destination.Type " +
                    " inner join fetch i.Destination.Category " +
                    " inner join fetch i.TransactionId ")
                    .List<Item>();
                return items;
            }
        }
   
    }
}
