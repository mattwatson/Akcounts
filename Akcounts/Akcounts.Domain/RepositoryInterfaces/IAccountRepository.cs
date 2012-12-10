using System.Collections.Generic;
using Akcounts.Domain.Objects;

namespace Akcounts.Domain.RepositoryInterfaces
{
    public interface IAccountRepository : IXmlRepository<Account>
    {
        Account GetByName(string name);
        IEnumerable<Account> GetByType(AccountType type);

        bool CouldSetAccountName(Account account, string name);
    }
}
