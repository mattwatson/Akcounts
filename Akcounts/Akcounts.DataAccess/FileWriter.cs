using System.IO;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.DataAccess
{
    public class FileWriter
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountTagRepository _accountTagRepository;
        private readonly IJournalRepository _journalRepository;
        private readonly ITemplateRepository _templateRepository;

        public FileWriter(IAccountRepository accountRepository, IAccountTagRepository accountTagRepository, ITemplateRepository templateRepository, IJournalRepository journalRepository)
        {
            _accountRepository = accountRepository;
            _accountTagRepository = accountTagRepository;
            _templateRepository = templateRepository;
            _journalRepository = journalRepository;
        }

        public void WriteAccountFile(string testDirectory)
        {
            var path = Path.Combine(testDirectory, _accountRepository.EntityNames + ".xml");
            var xml = _accountRepository.EmitXml();
            xml.Save(path);
        }
    }
}
