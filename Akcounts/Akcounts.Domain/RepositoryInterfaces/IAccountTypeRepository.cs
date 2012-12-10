using System;
using System.Collections.Generic;

namespace Akcounts.Domain
{
    public interface IAccountTypeRepository
    {
        void Add(AccountType account);
        void Update(AccountType account);
        void Remove(AccountType account);
        AccountType GetById(Guid accountId);
        AccountType GetByName(string name);
        ICollection<AccountType> GetAll();
    }
}
