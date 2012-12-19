using System.IO;
using System.Xml;
using System.Xml.Linq;
using Akcounts.DataAccess;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.MigrationTool
{
    class MigrationTool
    {
        static void Main()
        {
            IAccountTagRepository accountTagRepository = InitialiseAccountTagRepository("Data/accountTags.xml");
            IAccountRepository accountRepository = InitialiseAccountRepository("Data/accounts.xml", accountTagRepository);
            IJournalRepository journalRepository = InitialiseJournalRepository("Data/Journals.xml", accountRepository);

            var importer = new SpreadsheetImporter(accountTagRepository, accountRepository, journalRepository);
            importer.ImportSpreadsheet("MigrationData.xls");

            var fileWriter = new FileWriter(accountRepository, null, null, journalRepository);
            fileWriter.WriteAccountFile(@".\\Data");
            fileWriter.WriteJournalFile(@".\\Data");
        }
        
        private static AccountTagRepository InitialiseAccountTagRepository(string accountTagDataPath)
        {
            using (Stream accountTagStream = new FileStream(accountTagDataPath, FileMode.Open))
            using (XmlReader accountTagXml = new XmlTextReader(accountTagStream))
            {
                XElement accountTags = XElement.Load(accountTagXml);
                return new AccountTagRepository(accountTags);
            }
        }

        private static AccountRepository InitialiseAccountRepository(string accountDataPath, IAccountTagRepository accountTagRepository)
        {
            using (Stream accountStream = new FileStream(accountDataPath, FileMode.Open))
            using (XmlReader accountXml = new XmlTextReader(accountStream))
            {
                XElement accounts = XElement.Load(accountXml);
                return new AccountRepository(accounts, accountTagRepository);
            }
        }

        private static JournalRepository InitialiseJournalRepository(string journalDataPath, IAccountRepository accountRepository)
        {
            using (Stream journalStream = new FileStream(journalDataPath, FileMode.Open))
            using (XmlReader journalXml = new XmlTextReader(journalStream))
            {
                XElement journals = XElement.Load(journalXml);
                return new JournalRepository(journals, accountRepository);
            }
        }
    }
}
