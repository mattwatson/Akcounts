﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
            var fileContents = _accountRepository.EmitXml();

            WriteFile(targetDirectory, entityNames, fileContents);
        }

        public void WriteAccountTagFile(string targetDirectory)
        {
            var entityNames = _accountTagRepository.EntityNames;
            var fileContents = _accountTagRepository.EmitXml();

            WriteFile(targetDirectory, entityNames, fileContents);
        }

        public void WriteJournalFile(string targetDirectory)
        {
            var entityNames = _journalRepository.EntityNames;
            var fileContents = _journalRepository.EmitXml();

            WriteFile(targetDirectory, entityNames, fileContents);
        }

        public void WriteTemplateFile(string targetDirectory)
        {
            var entityNames = _templateRepository.EntityNames;
            var fileContents = _templateRepository.EmitXml();

            WriteFile(targetDirectory, entityNames, fileContents);
        }

        private void WriteFile(string targetDirectory, string entityNames, XStreamingElement fileContents)
        {
            var fileContentsWithHeader = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine + fileContents;

            var path = Path.Combine(targetDirectory, entityNames + ".xml");
            if (File.Exists(path))
            {
                var existingFileContents = File.ReadAllText(path);
                if (existingFileContents == fileContentsWithHeader)
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

            File.WriteAllText(path, fileContentsWithHeader);
        }

        private int GetFileNumber(string backupFolder, string entityNames)
        {
            var files = Directory.GetFiles(backupFolder, String.Format("*{0}*", entityNames));

            var maxFileNumber = files.Select(file => Int32.Parse(Regex.Match(file, @"(\d+)").Value)).Concat(new[] {0}).Max();
            
            return maxFileNumber + 1;
        }
    }
}
