using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akcounts.Domain;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Iesi.Collections.Generic;

namespace Akcounts.DataAccess
{
    public class ImportExport
    {

        private static void RecreateDatabase()
        {
            ISessionFactory _sessionFactory;
            Configuration _configuration;

            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.AddAssembly(typeof(Item).Assembly);
            _sessionFactory = _configuration.BuildSessionFactory();

            new SchemaExport(_configuration).Execute(false, true, false, false);
        }


        private static void ImportAccountTypes(OleDbConnection dbConnection, IDictionary<string, AccountType> at)
        {
            OleDbDataAdapter dataAdapter;
            DataSet dataSet = new DataSet();

            dataAdapter = new OleDbDataAdapter("SELECT * FROM [AccountTypes$]", dbConnection);

            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                AccountType accountType = new AccountType
                {
                    Name = row["Name"].ToString(),
                    IsDestination = bool.Parse(row["IsDestination"].ToString()),
                    IsSource = bool.Parse(row["IsSource"].ToString()),
                    IsValid = bool.Parse(row["IsValid"].ToString()),
                };
                at.Add(accountType.Name, accountType);
            }
        }

        private static void ImportAccountCategories(OleDbConnection dbConnection, IDictionary<string, AccountCategory> ac)
        {
            OleDbDataAdapter dataAdapter;
            DataSet dataSet = new DataSet();

            dataAdapter = new OleDbDataAdapter("SELECT * FROM [AccountCategories$]", dbConnection);

            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                AccountCategory accountCategory = new AccountCategory
                {
                    Name = row["Name"].ToString(),
                    Colour = row["Colour"].ToString(),
                    IsValid = bool.Parse(row["IsValid"].ToString()),
                };
                ac.Add(accountCategory.Name, accountCategory);
            }
        }

        private static void ImportAccounts(OleDbConnection dbConnection,
            IDictionary<string, Account> accounts,
            IDictionary<string, AccountType> accountTypes,
            IDictionary<string, AccountCategory> accountCategories)
        {

            OleDbDataAdapter dataAdapter;
            DataSet dataSet = new DataSet();

            dataAdapter = new OleDbDataAdapter("SELECT * FROM [Accounts$]", dbConnection);

            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Account account = new Account
                {
                    Name = row["Name"].ToString(),
                    IsValid = bool.Parse(row["IsValid"].ToString()),
                };
                account.setCategory(accountCategories[row["Category Name"].ToString()]);
                account.setType(accountTypes[row["Type Name"].ToString()]);

                accounts.Add(account.Name, account);
            }
        }

        private static void ImportTransactions(OleDbConnection dbConnection, IDictionary<int, Transaction> transactions)
        {

            OleDbDataAdapter dataAdapter;
            DataSet dataSet = new DataSet();

            dataAdapter = new OleDbDataAdapter("SELECT * FROM [Transactions$]", dbConnection);

            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                DateTime tranDate = DateTime.Parse(row["Date"].ToString());
                Int32 tranKey = Int32.Parse(row["BusinessKey"].ToString());

                Transaction transaction = new Transaction
                {
                    BusinessKey = tranKey,
                    Date = tranDate,
                    Description = row["Description"].ToString(),
                    IsVerified = bool.Parse(row["IsVerified"].ToString()),
                };
                transactions.Add(transaction.BusinessKey, transaction);
            }
        }

        private static void ImportItems(OleDbConnection dbConnection,
            Iesi.Collections.Generic.ISet<Item> items,
            IDictionary<string, Account> accounts,
            IDictionary<int, Transaction> transactions)
        {
            OleDbDataAdapter dataAdapter;
            DataSet dataSet = new DataSet();

            dataAdapter = new OleDbDataAdapter("SELECT * FROM [Items$]", dbConnection);
            dataAdapter.Fill(dataSet);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Account source = accounts[row["Source"].ToString()];
                Account destination = accounts[row["Destination"].ToString()];

                int tranKey = Int32.Parse(row["TransactionKey"].ToString());
                Decimal value = Decimal.Parse(row["Value"].ToString());

                Transaction transaction = transactions[tranKey];

                Item item = new Item
                {
                    Description = row["Description"].ToString(),
                    Value = value,
                    IsVerified = bool.Parse(row["IsVerified"].ToString()),
                };
                item.SetTransaction(transaction);
                item.SetSource(source);
                item.SetDestination(destination);

                items.Add(item);
            }
        }

        public static void ImportFromExcelFile(string filename)
        {

            IDictionary<string, AccountType> accountTypes = new Dictionary<string, AccountType>();
            IDictionary<string, AccountCategory> accountCategories = new Dictionary<string, AccountCategory>();
            IDictionary<string, Account> accounts = new Dictionary<string, Account>();
            Iesi.Collections.Generic.ISet<Item> items = new HashedSet<Item>();
            IDictionary<int, Transaction> transactions = new Dictionary<int, Transaction>();

            OleDbConnection excelConnection;

            excelConnection = new OleDbConnection(
                "Provider=Microsoft.ACE.OLEDB.12.0; " +
                "Data Source=" + filename + "; " +
                "Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"");

            excelConnection.Open();

            RecreateDatabase();
            ImportAccountTypes(excelConnection, accountTypes);
            ImportAccountCategories(excelConnection, accountCategories);
            ImportAccounts(excelConnection, accounts, accountTypes, accountCategories);
            ImportTransactions(excelConnection, transactions);
            ImportItems(excelConnection, items, accounts, transactions);

            AccountTypeRepository atr = new AccountTypeRepository();
            AccountCategoryRepository acr = new AccountCategoryRepository();
            AccountRepository ar = new AccountRepository();
            TransactionRepository tr = new TransactionRepository();
            ItemRepository ir = new ItemRepository();

            foreach (AccountType at in accountTypes.Values) atr.Add(at);
            foreach (AccountCategory ac in accountCategories.Values) acr.Add(ac);
            foreach (Account a in accounts.Values) ar.Add(a);
            foreach (Transaction t in transactions.Values) tr.Add(t);
            foreach (Item i in items) ir.Add(i);

            excelConnection.Close();
        }

        private static void ExportAccountTypes(OleDbConnection dbConnection)
        {

            OleDbCommand createTable = new OleDbCommand("CREATE TABLE `AccountTypes` (" +
                "`Name` LongText, " +
                "`IsSource` LongText, " +
                "`IsDestination` LongText, " +
                "`IsValid` LongText " +
                ")", dbConnection);
            createTable.ExecuteNonQuery();

            AccountTypeRepository repository = new AccountTypeRepository();

            var accountTypes = repository.GetAll();

            foreach (AccountType a in accountTypes)
            {
                string insertStatement = "INSERT INTO [AccountTypes] ([Name], [IsSource], [IsDestination], [IsValid]) VALUES ('" +
                    a.Name + "', '" +
                    a.IsSource + "', '" +
                    a.IsDestination + "', '" +
                    a.IsValid + "')";

                OleDbCommand insert = new OleDbCommand(insertStatement, dbConnection);
                insert.ExecuteNonQuery();
            }
        }
        private static void ExportAccountCategories(OleDbConnection dbConnection)
        {

            OleDbCommand createTable = new OleDbCommand("CREATE TABLE `AccountCategories` (" +
                "`Name` LongText, " +
                "`Colour` LongText, " +
                "`IsValid` LongText" +
                ")", dbConnection);
            createTable.ExecuteNonQuery();

            AccountCategoryRepository repository = new AccountCategoryRepository();

            var accountCategories = repository.GetAll();

            foreach (AccountCategory a in accountCategories)
            {
                string insertStatement = "INSERT INTO [AccountCategories] ([Name], [Colour], [IsValid]) VALUES ('" +
                    a.Name + "', '" +
                    a.Colour + "', '" +
                    a.IsValid + "')";

                OleDbCommand insert = new OleDbCommand(insertStatement, dbConnection);
                insert.ExecuteNonQuery();
            }

        }
        private static void ExportAccounts(OleDbConnection dbConnection)
        {
            OleDbCommand createTable = new OleDbCommand("CREATE TABLE `Accounts` (" +
                "`Name` LongText, " +
                "`Type Name` LongText, " +
                "`Category Name` LongText, " +
                "`IsValid` LongText" +
                ")", dbConnection);
            createTable.ExecuteNonQuery();

            AccountRepository repository = new AccountRepository();

            var accountCategories = repository.GetAll();

            foreach (Account a in accountCategories)
            {
                string insertStatement = "INSERT INTO [Accounts] ([Name], [Type Name], [Category Name], [IsValid]) VALUES ('" +
                    a.Name + "', '" +
                    a.Type.Name + "', '" +
                    a.Category.Name + "', '" +
                    a.IsValid + "')";

                OleDbCommand insert = new OleDbCommand(insertStatement, dbConnection);
                insert.ExecuteNonQuery();
            }

        }
        private static void ExportTransactions(OleDbConnection dbConnection)
        {

            OleDbCommand createTable = new OleDbCommand("CREATE TABLE `Transactions` (" +
                "`BusinessKey` LongText, " +
                "`Date` LongText, " +
                "`Description` LongText," +
                "`IsVerified` LongText" +
                ")", dbConnection);
            createTable.ExecuteNonQuery();

            TransactionRepository repository = new TransactionRepository();

            var transactions = repository.GetAll();

            foreach (Transaction t in transactions)
            {
                if (t.Description != null)
                {
                    t.Description = t.Description.Replace("'", "''");
                }
                string insertStatement = "INSERT INTO [Transactions] ([BusinessKey], [Date], [Description], [IsVerified]) VALUES ('" +
                    t.BusinessKey + "', '" +
                    t.Date.ToString("dd-MMM-yy") + "', '" +
                    t.Description + "', '" +
                    t.IsVerified + "')";

                OleDbCommand insert = new OleDbCommand(insertStatement, dbConnection);
                insert.ExecuteNonQuery();
            }
        }
        private static void ExportItems(OleDbConnection dbConnection)
        {

            OleDbCommand createTable = new OleDbCommand("CREATE TABLE `Items` (" +
                "`TransactionKey` LongText, " +
                "`Source` LongText, " +
                "`Destination` LongText, " +
                "`Value` LongText, " +
                "`Description` LongText, " +
                "`IsVerified` LongText" +
                ")", dbConnection);
            createTable.ExecuteNonQuery();

            ItemRepository repository = new ItemRepository();

            var items = repository.GetAll();

            foreach (Item i in items)
            {

                if (i.Description != null)
                {
                    i.Description = i.Description.Replace("'", "''");
                }

                string insertStatement = "INSERT INTO [Items] ([TransactionKey], [Source], [Destination], [Value], [Description], [IsVerified]) VALUES ('" +
                    i.TransactionId.BusinessKey + "', '" +
                    i.Source.Name + "', '" +
                    i.Destination.Name + "', '" +
                    i.Value.ToString() + "', '" +
                    i.Description + "', '" +
                    i.IsVerified + "')";

                OleDbCommand insert = new OleDbCommand(insertStatement, dbConnection);
                insert.ExecuteNonQuery();

            }

        }
        public static void ExportToExcelFile(string filename)
        {
            if (File.Exists(filename)) File.Delete(filename);

            OleDbConnection excelConnection;

            excelConnection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; " +
                "Data Source=" + filename + ";Extended Properties='Excel 8.0;HDR=Yes'");

            excelConnection.Open();

            ExportAccountCategories(excelConnection);
            ExportAccountTypes(excelConnection);
            ExportAccounts(excelConnection);
            ExportTransactions(excelConnection);
            ExportItems(excelConnection);

            excelConnection.Close();


        }
    }
}