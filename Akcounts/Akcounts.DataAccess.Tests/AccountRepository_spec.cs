using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using NMock2;
using NUnit.Framework;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class AccountRepository_spec
    {
        private IAccountRepository _repository;

        const string TestData = @"<accounts>
  <account id=""1"" name=""Cash"" type=""1"" isEnabled=""true"">
    <tags>
      <tag>1</tag>
      <tag>2</tag>
    </tags>
  </account>
  <account id=""2"" name=""HSBC Current"" type=""1"" isEnabled=""true"">
    <tags>
      <tag>3</tag>
      <tag>4</tag>
    </tags>
  </account>
  <account id=""3"" name=""Eating Out"" type=""4"" isEnabled=""true"">
    <tags>
      <tag>1</tag>
      <tag>4</tag>
    </tags>
  </account>
  <account id=""4"" name=""Rima"" type=""1"" isEnabled=""true"">
    <tags>
      <tag>2</tag>
    </tags>
  </account>
  <account id=""5"" name=""Payroll"" type=""3"" isEnabled=""false"">
    <tags />
  </account>
</accounts>";

        private readonly Account _account6 = new Account(6, "Credit Card", AccountType.Liability);
        private readonly Account _account7 = new Account(7, "Savings", AccountType.Asset);
        private readonly Account _account8 = new Account(8, "Pizza", AccountType.Expense);

        private readonly AccountTag _accountTag1 = new AccountTag(1, "Bank");
        private readonly AccountTag _accountTag2 = new AccountTag(2, "Holiday");
        private readonly AccountTag _accountTag3 = new AccountTag(3, "Transport");
        private readonly AccountTag _accountTag4 = new AccountTag(4, "Entertainment");

        private Mockery _mocks;
        private IAccountTagRepository _mockAccountTagRepository;

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccountTagRepository = _mocks.NewMock<IAccountTagRepository>();

            Expect.Exactly(2).On(_mockAccountTagRepository).Method("GetById").With(1).Will(Return.Value(_accountTag1));
            Expect.Exactly(2).On(_mockAccountTagRepository).Method("GetById").With(2).Will(Return.Value(_accountTag2));
            Expect.Once.On(_mockAccountTagRepository).Method("GetById").With(3).Will(Return.Value(_accountTag3));
            Expect.Exactly(2).On(_mockAccountTagRepository).Method("GetById").With(4).Will(Return.Value(_accountTag4));

            using (var accountStringReader = new StringReader(TestData))
            using (XmlReader accountXml = new XmlTextReader(accountStringReader))
            {
                XElement accounts = XElement.Load(accountXml);
                _repository = new AccountRepository(_mockAccountTagRepository);
            }

            _account6.AddTag(_accountTag3);
        }

        [Test]
        public void can_add_new_Account()
        {
            _repository.Save(_account6);
            var retrievedAccount = _repository.GetById(_account6.Id);

            Assert.AreEqual(_account6.Id, retrievedAccount.Id);
            Assert.AreEqual("Credit Card", retrievedAccount.Name);
            Assert.AreEqual(AccountType.Liability, retrievedAccount.Type);
            Assert.IsTrue(retrievedAccount.Tags.Contains(_accountTag3));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void adding_a_new_Account_without_an_id_gives_it_an_id_one_higher_than_maximum()
        {
            int maxId = _repository.GetAll().Select(n => n.Id).Max();

            var account = new Account(0, "Food - Eating out", AccountType.Expense);
            _repository.Save(account);

            Assert.AreEqual(maxId + 1, account.Id);
        }

        [Test]
        public void adding_a_new_Account_to_empty_repository_gives_it_an_id_of_one()
        {
            foreach (var a in _repository.GetAll().ToList())
                _repository.Remove(a);

            var account = new Account(0, "Food - Eating out", AccountType.Expense);
            _repository.Save(account);

            Assert.AreEqual(1, account.Id);
        }

        [Test]
        public void adding_an_account_with_no_name_gives_it_an_id()
        {
            var account = new Account(0, "", AccountType.Expense);
            _repository.Save(account);

            Assert.AreEqual(6, account.Id);
            Assert.IsFalse(account.IsValid);
        }

        [Test]
        public void account_only_gets_an_id_when_it_has_a_unique_name()
        {
            var duplicateAccount = new Account(0, "Cash", AccountType.Expense);

            Assert.Throws<EntityAlreadyExistsException>(() => _repository.Save(duplicateAccount));

            Assert.AreEqual(0, duplicateAccount.Id);
            Assert.Throws<EntityNotFoundException>(() => _repository.GetById(6));

            duplicateAccount.Name = "Different Name";
            _repository.Save(duplicateAccount);

            Assert.AreEqual(6, duplicateAccount.Id);
        }

        [Test]
        public void trying_to_add_an_account_with_a_duplicate_account_name_does_not_have_an_effect_on_repository()
        {
            var duplicateAccount = new Account(0, "Cash", AccountType.Expense);
            
            Assert.Throws<EntityAlreadyExistsException>(() => _repository.Save(duplicateAccount));

            Assert.AreEqual(0, duplicateAccount.Id);
            Assert.Throws<EntityNotFoundException>(() => _repository.GetById(0));
        }

        [Test]
        public void trying_to_edit_an_account_so_it_becomes_a_duplicate_account_name_does_not_have_an_effect_on_repository()
        {
            var account = new Account(0, "New name", AccountType.Expense);
            _repository.Save(account);
            var retrievedAccount = _repository.GetById(account.Id);

            Assert.Throws<EntityAlreadyExistsException>(() => retrievedAccount.Name = "Cash");
            var unchangedAccount = _repository.GetById(account.Id);

            Assert.AreEqual("New name", unchangedAccount.Name);
        }

        [Test]
        public void CouldSetAccountName_method_returns_true_if_account_name_could_be_added_to_repository()
        {
            Assert.IsTrue(_repository.CouldSetAccountName(_account6, "Camera"));
        }

        [Test]
        public void CouldSetAccountName_method_returns_false_if_account_name_is_a_duplicate()
        {
            Assert.IsFalse(_repository.CouldSetAccountName(_account6, "Cash"));
        }

        [Test]
        public void can_add_multiple_new_Accounts()
        {
            _repository.Save(_account6);
            _repository.Save(_account7);
            _repository.Save(_account8);
            var retrieved6 = _repository.GetById(_account6.Id);
            var retrieved7 = _repository.GetById(_account7.Id);
            var retrieved8 = _repository.GetById(_account8.Id);

            Assert.AreEqual(_account6.Id, retrieved6.Id);
            Assert.AreEqual("Credit Card", retrieved6.Name);
            Assert.AreEqual(AccountType.Liability, retrieved6.Type);

            Assert.AreEqual(_account7.Id, retrieved7.Id);
            Assert.AreEqual("Savings", retrieved7.Name);
            Assert.AreEqual(AccountType.Asset, retrieved7.Type);

            Assert.AreEqual(_account8.Id, retrieved8.Id);
            Assert.AreEqual("Pizza", retrieved8.Name);
            Assert.AreEqual(AccountType.Expense, retrieved8.Type);
        }

        [Test]
        public void inserting_same_Account_twice_has_no_side_affect()
        {
            _repository.Save(_account6);
            var accountTotal = _repository.GetAll().Count();

            _repository.Save(_account6);

            Assert.AreEqual(accountTotal, _repository.GetAll().Count());
        }

        [Test]
        public void inserting_Account_with_same_name_as_existing_account_causes_AccountAlreadyExistsException()
        {
            var existingAccount = _repository.GetById(2);
            var newAccount = new Account(2000, existingAccount.Name, AccountType.Payable);

            Assert.Throws<EntityAlreadyExistsException>(() => _repository.Save(newAccount));
            Assert.Throws<EntityNotFoundException>(() => _repository.GetById(newAccount.Id));
        }

        [Test]
        public void can_update_existing_Account()
        {
            var account = _repository.GetById(1);

            account.Name = "Regular Savings";
            account.IsEnabled = false;
            account.Type = AccountType.Payable;
            account.RemoveTag(_accountTag1);
            account.AddTag(_accountTag4);

            var updatedAccount = _repository.GetById(1);

            Assert.AreEqual("Regular Savings", updatedAccount.Name);
            Assert.AreEqual(false, updatedAccount.IsEnabled);
            Assert.AreEqual(AccountType.Payable, updatedAccount.Type);
            Assert.AreEqual(2, updatedAccount.Tags.Count);
            Assert.IsTrue(updatedAccount.Tags.Contains(_accountTag2));
            Assert.IsTrue(updatedAccount.Tags.Contains(_accountTag4));
        }

        [Test]
        public void can_update_existing_Account_to_same_name()
        {
            var account = _repository.GetById(1);

            account.Name = account.Name;
        }

        [Test]
        public void updating_that_would_create_a_duplicate_causes_AccountAlreadyExistsException()
        {
            var editableAccount1 = _repository.GetById(1);
            var account2 = _repository.GetById(2);
            
            Assert.Throws<EntityAlreadyExistsException>(() => editableAccount1.Name = account2.Name);
        }

        [Test]
        public void Can_get_all_remove_all_then_add_some_more_then_get_all_again()
        {
            var accounts = _repository.GetAll().ToList();

            foreach (var account in accounts)
                _repository.Remove(account);

            _repository.Save(_account6);
            _repository.Save(_account7);
            _repository.Save(_account8);

            var newAccounts = _repository.GetAll().OrderBy(x => x.Name).ToList();
            var account1n = newAccounts.ElementAt(0);
            var account2n = newAccounts.ElementAt(1);
            var account3n = newAccounts.ElementAt(2);

            Assert.AreEqual(3, newAccounts.Count);

            Assert.AreEqual(_account6.Id, account1n.Id);
            Assert.AreEqual("Credit Card", account1n.Name);
            Assert.AreEqual(AccountType.Liability, account1n.Type);

            Assert.AreEqual(_account8.Id, account2n.Id);
            Assert.AreEqual("Pizza", account2n.Name);
            Assert.AreEqual(AccountType.Expense, account2n.Type);

            Assert.AreEqual(_account7.Id, account3n.Id);
            Assert.AreEqual("Savings", account3n.Name);
            Assert.AreEqual(AccountType.Asset, account3n.Type);
        }

        [Test]
        public void Can_get_existing_Account_by_type()
        {
            _repository.Save(_account6);
            _repository.Save(_account7);
            var account3 = _repository.GetById(3);

            account3.Type = AccountType.Liability;
            _repository.Save(account3);

            var accounts = _repository.GetByType(AccountType.Liability).ToList();
            var accountIds = accounts.Select(x => x.Id).ToList();

            Assert.AreEqual(2, accounts.Count());
            Assert.IsTrue(accountIds.Contains(_account6.Id));
            Assert.IsTrue(accountIds.Contains(account3.Id));
            Assert.IsFalse(accountIds.Contains(_account7.Id));
        }

        [Test]
        public void Can_get_existing_Account_by_name()
        {
            _repository.Save(_account6);
            _repository.Save(_account7);
            var account3 = _repository.GetById(3);

            var retrievedAccount = _repository.GetByName(account3.Name);

            Assert.AreEqual(account3, retrievedAccount);
        }

        [Test]
        public void GetByName_returns_null_if_name_is_not_found()
        {
            _repository.Save(_account6);
            _repository.Save(_account7);

            var retrievedAccount = _repository.GetByName("nonexisting");
            
            Assert.IsNull(retrievedAccount);
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();

            Assert.AreEqual(TestData, output);
        }
    }
}
