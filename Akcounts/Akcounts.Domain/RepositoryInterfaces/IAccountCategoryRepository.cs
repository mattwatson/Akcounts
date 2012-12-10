using System;
using System.Collections.Generic;

namespace Akcounts.Domain
{
    public interface IAccountCategoryRepository
    {
        void Add(AccountCategory account);
        void Update(AccountCategory account);
        void Remove(AccountCategory account);
        AccountCategory GetById(Guid accountId);
        AccountCategory GetByName(string name);
        ICollection<AccountCategory> GetAll();
    }
}