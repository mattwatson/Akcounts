using System.Threading;
using System.Xml.Linq;
using Akcounts.Domain.RepositoryInterfaces;
using NSubstitute;
using NUnit.Framework;
using System.IO;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class FileWriter_spec
    {
        private const string TestDirectory = @".\TestFiles";

        private IAccountRepository _accountRepository;
        private IAccountTagRepository _accountTagRepository;
        private IJournalRepository _journalRepository;
        private ITemplateRepository _templateRepository;
        private FileWriter _fileWriter;
        private XStreamingElement _accountXml;
        private string _expectedAccountPath;

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }

            Directory.CreateDirectory(TestDirectory);

            SetUpAccountRepositoryTestData();

            _accountTagRepository = Substitute.For<IAccountTagRepository>();
            _journalRepository = Substitute.For<IJournalRepository>();
            _templateRepository = Substitute.For<ITemplateRepository>();
            
            _fileWriter = new FileWriter(_accountRepository, _accountTagRepository, _templateRepository, _journalRepository);
        }

        private void SetUpAccountRepositoryTestData()
        {
            _accountXml = new XStreamingElement("test", "someContent");
            _expectedAccountPath = Path.Combine(TestDirectory, "accounts.xml");
            _accountRepository = Substitute.For<IAccountRepository>();
            _accountRepository.EntityNames.Returns("accounts");
            _accountRepository.EmitXml().Returns(_accountXml);
        }

        [Test]
        public void WriteAccounts_creates_Account_file_in_the_specified_directory()
        {
            _fileWriter.WriteAccountFile(TestDirectory);
            
            Assert.IsTrue(File.Exists(_expectedAccountPath), "Expected file was not found");
        }

        [Test]
        public void WriteAccounts_does_not_overwrite_Account_file_if_it_has_not_changed()
        {
            _fileWriter.WriteAccountFile(TestDirectory);
            var originalTimestamp = File.GetLastWriteTimeUtc(_expectedAccountPath);

            Thread.Sleep(10);

            _fileWriter.WriteAccountFile(TestDirectory);
            var timestampAfterSecondWrite = File.GetLastWriteTimeUtc(_expectedAccountPath);

            Assert.AreEqual(originalTimestamp, timestampAfterSecondWrite);
        }

        [Test]
        public void WriteAccounts_creates_a_backup_and_overwrites_Account_file_if_it_has_changed()
        {
            _fileWriter.WriteAccountFile(TestDirectory);
            var originalTimestamp = File.GetLastWriteTimeUtc(_expectedAccountPath);

            Thread.Sleep(10);

            var modifiedAccountXml = new XStreamingElement("test", "someDifferentContent");
            _accountRepository.EmitXml().Returns(modifiedAccountXml);
            _fileWriter.WriteAccountFile(TestDirectory);

            var timestampAfterSecondWrite = File.GetLastWriteTimeUtc(_expectedAccountPath);
            
            Assert.AreNotEqual(originalTimestamp, timestampAfterSecondWrite);

            var backupPath = Path.Combine(TestDirectory, @"backup\", "accounts.backup-1.xml");

            Assert.IsTrue(File.Exists(backupPath));
            Assert.AreEqual(_accountXml.ToString(), File.ReadAllText(backupPath));
            Assert.AreEqual(originalTimestamp, File.GetLastWriteTimeUtc(backupPath));
        }

        [Test]
        public void WriteAccounts_creates_a_new_backup_each_time_the_file_is_overwritten()
        {
            var backupPath = Path.Combine(TestDirectory, @"backup\");
            Directory.CreateDirectory(backupPath);

            File.WriteAllText(Path.Combine(backupPath, "accounts.backup-12.xml"), "dummyFile");
            var modifiedAccountXml = new XStreamingElement("test", "someDifferentContent");

            _fileWriter.WriteAccountFile(TestDirectory);
            
            _accountRepository.EmitXml().Returns(modifiedAccountXml);
            _fileWriter.WriteAccountFile(TestDirectory);

            _accountRepository.EmitXml().Returns(_accountXml);
            _fileWriter.WriteAccountFile(TestDirectory);

            Assert.IsTrue(File.Exists(Path.Combine(TestDirectory, @"backup\", "accounts.backup-13.xml")));
            Assert.IsTrue(File.Exists(Path.Combine(TestDirectory, @"backup\", "accounts.backup-14.xml")));
            Assert.IsFalse(File.Exists(Path.Combine(TestDirectory, @"backup\", "accounts.backup-16.xml")));
        }

    }
}