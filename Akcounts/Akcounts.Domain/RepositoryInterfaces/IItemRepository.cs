using System;
using System.Collections.Generic;

namespace Akcounts.Domain
{
    public interface IItemRepository
    {
        void Add(Item account);
        void Update(Item account);
        void Remove(Item account);
        Item GetById(Guid accountId);
        ICollection<Item> GetByTransaction(Transaction transaction);
        ICollection<Item> GetByAccount(Account account);
        ICollection<Item> GetBySourceAccount(Account account);
        ICollection<Item> GetByDestinationAccount(Account account);
        ICollection<Item> GetAll();
        ICollection<Item> GetByDate(DateTime transactionDate);
        ICollection<Item> GetByDate(DateTime startDate, DateTime endDate);
        ICollection<Item> GetByDate(DateTime startDate, DateTime endDate, bool includeValid, bool includeInvalid);
    }
}
