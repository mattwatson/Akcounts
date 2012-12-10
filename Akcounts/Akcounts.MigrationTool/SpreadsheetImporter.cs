using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.MigrationTool
{
    class SpreadsheetImporter
    {
        private OleDbConnection _excelConnection;

        private readonly IDictionary<string, AccountTag> _accountTags = new Dictionary<string, AccountTag>();
        private readonly IAccountRepository _accountRepository;
        private readonly IJournalRepository _journalRepository;

        private IDictionary<int, Journal> _journals;
        
        public SpreadsheetImporter(IAccountTagRepository accountTagRepository, IAccountRepository accountRepository, IJournalRepository journalRepository)
        {
            _accountRepository = accountRepository;
            _journalRepository = journalRepository;

            foreach (var tag in accountTagRepository.GetAll())
            {
                _accountTags.Add(tag.Name, tag);
            }
        }

        internal void ImportSpreadsheet(string filename)
        {
            var connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"", filename);
            using (_excelConnection = new OleDbConnection(connectionString))
            {
                ImportAccounts();
                ImportJournals();
                ImportTransactions();
            }
            
            foreach (var journal in _journals.Values) 
                _journalRepository.Save(journal);
        }

        private void ImportAccounts()
        {
            var dataAdapter = new OleDbDataAdapter("SELECT * FROM [Accounts$]", _excelConnection);
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var name = row["Name"].ToString();
                var type = ConvertAccountType(row["Type Name"].ToString());
                var tag = _accountTags[row["Category Name"].ToString()];

                var account = _accountRepository.GetByName(name) ?? new Account(0, name, type);
                account.AddTag(tag);

                _accountRepository.Save(account);
            }
        }
        
        private void ImportJournals()
        {
            var dataAdapter = new OleDbDataAdapter("SELECT * FROM [Transactions$]", _excelConnection);
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            _journals = new Dictionary<int, Journal>();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {

                var date = DateTime.Parse(row["Date"].ToString());
                var id = Int32.Parse(row["BusinessKey"].ToString());
                var description = row["Description"].ToString();

                var journal = new Journal(date, description);

                _journals.Add(id, journal);
            }
        }

        private void ImportTransactions()
        {
            var dataSet = new DataSet();
            var dataAdapter = new OleDbDataAdapter("SELECT * FROM [Items$]", _excelConnection);
            dataAdapter.Fill(dataSet);
            
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var journal = _journals[Int32.Parse(row["TransactionKey"].ToString())];

                var fromAccount = _accountRepository.GetByName(row["Source"].ToString());
                var toAccount = _accountRepository.GetByName(row["Destination"].ToString());

                var value = Decimal.Parse(row["Value"].ToString());
                var note = row["Description"].ToString();

                new Transaction(journal, TransactionDirection.Out, fromAccount, value, note);
                new Transaction(journal, TransactionDirection.In, toAccount, value, note);
            }
        }

        private AccountType ConvertAccountType(string typeName)
        {
            switch (typeName)
            {
                case "AccountPayable":
                    return AccountType.Payable;
                case "Liability":
                    return AccountType.Liability;
                case "Asset":
                    return AccountType.Asset;
                case "Equity":
                    return AccountType.Equity;
                case "Expense":
                    return AccountType.Expense;
                case "Income":
                    return AccountType.Income;
                case "AccountRecievable":
                    return AccountType.Receivable;
                default:
                    throw new ArgumentOutOfRangeException(typeName);
            }
        }
    }
}
