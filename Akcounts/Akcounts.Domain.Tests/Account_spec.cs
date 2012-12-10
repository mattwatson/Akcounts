using System;
using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class Account_spec
    {
        private readonly AccountTag _accountTag1 = new AccountTag(1, "Savings");
        private readonly AccountTag _accountTag2 = new AccountTag(2, "Halifax");
        private readonly AccountTag _accountTag3 = new AccountTag(3, "Volatile");
        private readonly AccountTag _accountTag4 = new AccountTag(4, "HSBC");

        [Test]
        public void can_create_new_Account()
        {
            var account = new Account(1, "Bank Account", AccountType.Asset);

            Assert.AreEqual(1, account.Id);
            Assert.AreEqual("Bank Account", account.Name);
            Assert.AreEqual(AccountType.Asset, account.Type);
            Assert.IsTrue(account.IsValid);
        }

        [Test]
        public void creating_an_account_with_id_of_zero_causes_IsValid_to_be_false()
        {
            const string name = "Holiday";
            const AccountType type = AccountType.Expense;
            var account1 = new Account(1, name, type);
            var account2 = new Account(0, name, type);
            
            Assert.IsTrue(account1.IsValid);
            Assert.IsFalse(account2.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void an_account_created_with_no_name_is_not_valid(string name)
        {
            const int id = 1;
            const AccountType type = AccountType.Expense;
            var account1 = new Account(id, "Holiday", type);
            var account2 = new Account(id, name, type);
            
            Assert.IsTrue(account1.IsValid);
            Assert.IsFalse(account2.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void setting_a_blank_name_makes_an_account_invalid(string name)
        {
            var account = new Account(1, "Halifax Savings", AccountType.Asset);
            Assert.IsTrue(account.IsValid);

            account.Name = name;
            Assert.IsFalse(account.IsValid);
        }

        [Test]
        public void can_change_AccountType_on_an_Account()
        {
            var account = new Account(1, "Cash", AccountType.Asset) {Type = AccountType.Liability};

            Assert.AreEqual(AccountType.Liability, account.Type);
            Assert.IsTrue(account.IsValid);
        }

        [TestCase(1000)]
        [TestCase(0)]
        [TestCase(-1)]
        public void cannot_create_new_Account_with_invalid_AccountType(int invalid)
        {
            Assert.Throws<ArgumentException>(() => new Account(1, "Bank Account", (AccountType)invalid));
        }

        [TestCase(1000)]
        [TestCase(0)]
        [TestCase(-1)]
        public void cannot_set_the_account_type_to_an_invalid_value(int invalid)
        {
            var account = new Account(1, "Cash", AccountType.Asset);

            Assert.Throws<ArgumentException>(() => account.Type = (AccountType)invalid);
        }

        [Test]
        public void newly_created_Account_is_enabled()
        {
            var account = new Account(1, "Barclays ISA", AccountType.Asset);
            Assert.IsTrue(account.IsEnabled);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Account_can_be_marked_as_enabled_or_disabled(bool isEnabled)
        {
            var account = new Account(1, "Barclays ISA", AccountType.Asset) {IsEnabled = isEnabled};

            Assert.AreEqual(account.IsEnabled, isEnabled);
        }

        [Test]
        public void ToString_method_returns_name()
        {
            var account = new Account(1, "Barclays ISA", AccountType.Receivable);
            Assert.AreEqual(account.Name, account.ToString());
        }

        [Test]
        public void can_add_tags_to_an_account()
        {
            var account = new Account(1, "Halifax - SIF ISA", AccountType.Asset);

            Assert.AreEqual(0, account.Tags.Count);
            Assert.IsFalse(account.Tags.Contains(_accountTag1));
            Assert.IsFalse(account.Tags.Contains(_accountTag2));

            account.AddTag(_accountTag1);
            Assert.AreEqual(1, account.Tags.Count);
            Assert.IsTrue(account.Tags.Contains(_accountTag1));
            Assert.IsFalse(account.Tags.Contains(_accountTag2));

            account.AddTag(_accountTag2);
            Assert.AreEqual(2, account.Tags.Count);
            Assert.IsTrue(account.Tags.Contains(_accountTag1));
            Assert.IsTrue(account.Tags.Contains(_accountTag2));
        }

        [Test]
        public void can_clear_tags()
        {
            var account = new Account(2, "Halifax - SIF ISA", AccountType.Asset);

            account.AddTag(_accountTag1);
            account.AddTag(_accountTag2);

            account.ClearTags();
            Assert.AreEqual(0, account.Tags.Count);
        }

        [Test]
        public void adding_the_same_tag_twice_does_not_result_in_duplicates_but_moves_tag_to_the_end()
        {
            var account = new Account(10001, "Halifax - SIF ISA", AccountType.Asset);

            account.AddTag(_accountTag1);
            account.AddTag(_accountTag1);

            Assert.AreEqual(1, account.Tags.Count);
            Assert.AreEqual(_accountTag1, account.Tags[0]);

            account.AddTag(_accountTag2);
            Assert.AreEqual(2, account.Tags.Count);
            Assert.AreEqual(_accountTag1, account.Tags[0]);
            Assert.AreEqual(_accountTag2, account.Tags[1]);

            account.AddTag(_accountTag1);
            Assert.AreEqual(2, account.Tags.Count);
            Assert.AreEqual(_accountTag2, account.Tags[0]);
            Assert.AreEqual(_accountTag1, account.Tags[1]);

            account.AddTag(_accountTag3);
            Assert.AreEqual(3, account.Tags.Count);
            Assert.AreEqual(_accountTag2, account.Tags[0]);
            Assert.AreEqual(_accountTag1, account.Tags[1]);
            Assert.AreEqual(_accountTag3, account.Tags[2]);

            account.AddTag(_accountTag2);
            Assert.AreEqual(3, account.Tags.Count);
            Assert.AreEqual(_accountTag1, account.Tags[0]);
            Assert.AreEqual(_accountTag3, account.Tags[1]);
            Assert.AreEqual(_accountTag2, account.Tags[2]);
        }

        [Test]
        public void can_remove_a_tag_from_an_account()
        {
            var account = new Account(1, "Halifax - SIF ISA", AccountType.Asset);

            account.AddTag(_accountTag1);
            account.AddTag(_accountTag2);

            account.RemoveTag(_accountTag1);

            Assert.AreEqual(1, account.Tags.Count);
            Assert.IsFalse(account.Tags.Contains(_accountTag1));
            Assert.IsTrue(account.Tags.Contains(_accountTag2));
        }

        [Test]
        public void FirstMatchingTag_returns_null_if_no_tags_match()
        {
            var account = new Account(1, "Halifax - SIF ISA", AccountType.Asset);

            var tagFilter = new System.Collections.Generic.List<AccountTag> {_accountTag1, _accountTag2};

            var firstMatchingTag = account.FirstMatchingTag(tagFilter);
            Assert.IsNull(firstMatchingTag);
        }

        [Test]
        public void FirstMatchingTag_returns_first_tag_that_matches_any_of_the_filters()
        {
            var account1 = new Account(1, "Halifax - SIF ISA", AccountType.Asset);
            var account2 = new Account(2, "HSBC - Savings", AccountType.Asset);

            account1.AddTag(_accountTag1);
            account1.AddTag(_accountTag2);

            account2.AddTag(_accountTag4);
            account2.AddTag(_accountTag1);

            var tagFilter = new System.Collections.Generic.List<AccountTag> {_accountTag1, _accountTag2};

            var firstMatchingTag1 = account1.FirstMatchingTag(tagFilter);
            var firstMatchingTag2 = account2.FirstMatchingTag(tagFilter);

            Assert.AreEqual(_accountTag1, firstMatchingTag1);
            Assert.AreEqual(_accountTag1, firstMatchingTag2);    
        }

        [Test]
        public void can_emit_xml_describing_Account()
        {
            var account = new Account(3, "Eating Out", AccountType.Expense) {IsEnabled = false};
            account.AddTag(_accountTag2);
            account.AddTag(_accountTag3);

            const string expectedResult = @"<account id=""3"" name=""Eating Out"" type=""4"" isEnabled=""false"">
  <tags>
    <tag>2</tag>
    <tag>3</tag>
  </tags>
</account>";

            Assert.AreEqual(expectedResult, account.EmitXml().ToString());
        }

        [Test]
        public void can_add_validation_to_accounts_Name_setter_which_is_called_when_account_Name_is_changed()
        {
            bool validatorCalled = false;
            var account = new Account(3, "Eating Out", AccountType.Expense);

            account.NameChanged += ((s, a) =>
            {
                validatorCalled = true;
            });

            account.Name = "Restaurant Meal";
            Assert.AreEqual("Restaurant Meal", account.Name);
            Assert.IsTrue(validatorCalled);
        }

        [Test]
        public void when_validation_of_accounts_Name_setter_fails_the_name_is_not_updated()
        {
            bool validatorCalled = false;
            var account = new Account(3, "Eating Out", AccountType.Expense);

            account.NameChanged += ((s, a) =>
            {
                validatorCalled = true;
                throw new EntityAlreadyExistsException();
            });

            Assert.Throws<EntityAlreadyExistsException> (() => account.Name = "Restaurant Meal");

            Assert.AreEqual("Eating Out", account.Name);
            Assert.IsTrue(validatorCalled);
        }
    }
}
