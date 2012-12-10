using Akcounts.Domain.Objects;

namespace Akcounts.Domain.RepositoryInterfaces
{
    public interface IAccountTagRepository : IXmlRepository<AccountTag>
    {
        AccountTag AddNewAccountTag(string name);

        bool CouldSetAccountTagName(AccountTag tag, string name);
    }
}
