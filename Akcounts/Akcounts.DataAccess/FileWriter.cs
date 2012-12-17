using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        public void WriteAccountFile(string targetDirectory)
        {
            var entityNames = _accountRepository.EntityNames;
            var path = Path.Combine(targetDirectory, entityNames + ".xml");
            var newFileContents = _accountRepository.EmitXml().ToString();

            if (File.Exists(path))
            {
                var existingFileContents = File.ReadAllText(path);
                if (existingFileContents == newFileContents)
                {
                    return;
                }

                var backupFolder = Path.Combine(targetDirectory, @"backup\");
                Directory.CreateDirectory(backupFolder);

                var fileNumber = GetFileNumber(backupFolder, entityNames);
                var backupFileName = String.Format("{0}.backup-{1}.xml", entityNames, fileNumber);

                var backupPath = Path.Combine(backupFolder, backupFileName);
                File.Move(path, backupPath);
            }
            
            File.WriteAllText(path, newFileContents);
        }

        private int GetFileNumber(string backupFolder, string entityNames)
        {
            var files = Directory.GetFiles(backupFolder, String.Format("*{0}*", entityNames));

            var maxFileNumber = files.Select(file => Int32.Parse(Regex.Match(file, @"(\d+)").Value)).Concat(new[] {0}).Max();
            
            return maxFileNumber + 1;
        }
    }
}
