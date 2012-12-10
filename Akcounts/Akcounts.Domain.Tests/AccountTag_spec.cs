using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class AccountTag_spec
    {
        [Test]
        public void can_create_new_accountTag()
        {
            var accountTag = new AccountTag(1, "Holiday");

            Assert.AreEqual(1, accountTag.Id);
            Assert.AreEqual("Holiday", accountTag.Name);
            Assert.IsTrue(accountTag.IsValid);
        }

        [Test]
        public void can_set_id_to_zero_when_creating_AccountTags_but_then_IsValid_is_false()
        {
            var accountTag = new AccountTag(0, "Holiday");
            Assert.AreEqual(0, accountTag.Id);
            Assert.IsFalse(accountTag.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void you_can_create_an_AccountTag_with_no_name_but_then_IsValid_is_false(string name)
        {
            var tag = new AccountTag(100, name);

            Assert.AreEqual(name, tag.Name);
            Assert.IsFalse(tag.IsValid);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void can_set_AccountTag_name_to_blank_or_null_but_then_IsValid_is_false(string name)
        {
            var accountTag = new AccountTag(101, "Holiday - USA 2010") {Name = name};

            Assert.AreEqual(name, accountTag.Name);
            Assert.IsFalse(accountTag.IsValid);
        }

        [Test]
        public void account_tags_are_equal__when_id_is_equal()
        {
            var accountTag1 = new AccountTag(2, "Food");
            var accountTag2 = new AccountTag(2, "Bonanza");

            Assert.IsTrue(accountTag1.Equals(accountTag2));
            Assert.IsTrue(accountTag1 == accountTag2);
            Assert.AreEqual(accountTag1, accountTag2);
            Assert.AreNotSame(accountTag1, accountTag2);
        }

        [Test]
        public void account_tags_are_not_equal_if_id_is_different()
        {
            var accountTag1 = new AccountTag(1, "Food");
            var accountTag2 = new AccountTag(2, "Food");

            Assert.IsFalse(accountTag1.Equals(accountTag2));
            Assert.IsFalse(accountTag1 == accountTag2);
            Assert.AreNotEqual(accountTag1, accountTag2);
            Assert.AreNotSame(accountTag1, accountTag2);
        }

        [Test]
        public void to_string_method_returns_name()
        {
            var tag = new AccountTag(1, "ISA");
            Assert.AreEqual(tag.Name, tag.ToString());
        }

        [Test]
        public void can_emit_xml_describing_AccountTag()
        {
            var tag = new AccountTag(3, "Tax free Savings");

            const string expectedResult = @"<tag id=""3"">Tax free Savings</tag>";

            Assert.AreEqual(expectedResult, tag.EmitXml().ToString());
        }

        [Test]
        public void can_add_validation_to_accountTags_Name_setter_which_is_called_when_accountTag_Name_is_changed()
        {
            bool validatorCalled = false;
            var tag = new AccountTag(3, "ISAs");

            tag.NameChanged += ((s, a) =>
            {
                validatorCalled = true;
            });

            tag.Name = "ISAs";
            Assert.AreEqual("ISAs", tag.Name);
            Assert.IsTrue(validatorCalled);
        }

        [Test]
        public void when_validation_of_accountTags_Name_setter_fails_the_name_is_not_updated()
        {
            bool validatorCalled = false;
            var tag = new AccountTag(3, "Tax free Savings");

            tag.NameChanged += ((s, a) =>
            {
                validatorCalled = true;
                throw new EntityAlreadyExistsException();
            });

            Assert.Throws<EntityAlreadyExistsException>(() => tag.Name = "ISAs");

            Assert.AreEqual("Tax free Savings", tag.Name);
            Assert.IsTrue(validatorCalled);
        }
    }
}
