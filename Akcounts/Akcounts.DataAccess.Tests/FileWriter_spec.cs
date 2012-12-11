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

        [SetUp]
        public void SetUp()
        {
            _accountRepository = Substitute.For<IAccountRepository>();
            _accountTagRepository = Substitute.For<IAccountTagRepository>();
            _journalRepository = Substitute.For<IJournalRepository>();
            _templateRepository = Substitute.For<ITemplateRepository>();
            _fileWriter = new FileWriter(_accountRepository, _accountTagRepository, _templateRepository, _journalRepository);

            Directory.CreateDirectory(TestDirectory);
        }

        [Test]
        public void WriteAccounts_creates_Account_file_in_the_specified_directory()
        {
            var accountXml = new XStreamingElement("test", "someContent");
            _accountRepository.EmitXml().Returns(accountXml);
            _accountRepository.EntityNames.Returns("accounts");

            _fileWriter.WriteAccountFile(TestDirectory);
            
            var expectedPath = Path.Combine(TestDirectory, "accounts.xml");
            Assert.IsTrue(File.Exists(expectedPath), "Expected file was not found");
        }
    }
}