using System.Linq;
using System.Xml;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using NUnit.Framework;
using System.IO;
using System.Xml.Linq;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class AccountTagRepository_spec
    {
        IAccountTagRepository _repository;

        const string TestData = @"<accountTags>
  <tag id=""1"">Entertainment</tag>
  <tag id=""2"">Transport</tag>
  <tag id=""3"">Items</tag>
  <tag id=""4"">Food</tag>
  <tag id=""5"">Other</tag>
  <tag id=""6"">Tax</tag>
  <tag id=""7"">Person</tag>
  <tag id=""8"">Services</tag>
  <tag id=""9"">Cash</tag>
  <tag id=""10"">Loan</tag>
  <tag id=""11"">Bills</tag>
  <tag id=""12"">Pay</tag>
  <tag id=""13"">Bank</tag>
</accountTags>";

        private readonly AccountTag _accountTag1 = new AccountTag(16, "Bank - Current");
        private readonly AccountTag _accountTag2 = new AccountTag(23, "Holiday");

        [SetUp]
        public void SetUp()
        {
            using (var accountTagStringReader = new StringReader(TestData))
            using (XmlReader accountTagXml = new XmlTextReader(accountTagStringReader))
            {
                XElement accountTags = XElement.Load(accountTagXml);
                _repository = new AccountTagRepository();
            }
        }

        [Test]
        public void can_add_new_AccountTag()
        {
            _repository.Save(_accountTag1);
            var tag = _repository.GetById(_accountTag1.Id);

            Assert.AreEqual(_accountTag1.Id, tag.Id);
            Assert.AreEqual("Bank - Current", tag.Name);
        }

        [Test]
        public void adding_a_new_AccountTag_without_an_id_gives_it_an_id_one_higher_than_maximum()
        {
            int maxId = _repository.GetAll().Select(n => n.Id).Max();

            var newTag = new AccountTag(0, "Eating out");
            _repository.Save(newTag);

            Assert.AreEqual(maxId + 1, newTag.Id);
        }

        [Test]
        public void adding_a_new_AccountTag_to_empty_repository_gives_it_an_id_of_one()
        {
            foreach (var tag in _repository.GetAll().ToList())
                _repository.Remove(tag);

            var newTag = new AccountTag(0, "Food - Eating out");
            _repository.Save(newTag);

            Assert.AreEqual(1, newTag.Id);
        }

        [Test]
        public void can_add_multiple_new_AccountTags()
        {
            _repository.Save(_accountTag1);
            _repository.Save(_accountTag2);
            var tag1 = _repository.GetById(_accountTag1.Id);
            var tag2 = _repository.GetById(_accountTag2.Id);

            Assert.AreEqual(_accountTag1.Id, tag1.Id);
            Assert.AreEqual("Bank - Current", tag1.Name);

            Assert.AreEqual(_accountTag2.Id, tag2.Id);
            Assert.AreEqual("Holiday", tag2.Name);
        }

        [Test]
        public void inserting_same_AccountTag_twice_has_no_side_effect()
        {
            _repository.Save(_accountTag1);
            var tagCount = _repository.GetAll().Count();
            _repository.Save(_accountTag1);

            Assert.AreEqual(tagCount, _repository.GetAll().Count());
        }

        [Test]
        public void inserting_AccountTag_with_same_name_as_existing_AccountTag_causes_AccountTagAlreadyExistsException()
        {
            var existingTag = _repository.GetById(2);
            var newTag = new AccountTag(2000, existingTag.Name);

            Assert.Throws<EntityAlreadyExistsException>(() => _repository.Save(newTag));
        }

        [Test]
        public void can_update_existing_AccountTag()
        {
            var tag = _repository.GetById(1);
            tag.Name = "Savings";

            var updatedTag = _repository.GetById(1);

            Assert.AreEqual("Savings", tag.Name);
            Assert.AreEqual("Savings", updatedTag.Name);
        }

        [Test]
        public void can_update_existing_AccountTag_to_the_same_name()
        {
            var tag = _repository.GetById(1);
            tag.Name = tag.Name;
        }

        [Test]
        public void updating_that_would_create_a_duplicate_causes_AccountTagAlreadyExistsException()
        {
            var editableTag = _repository.GetById(1);
            var tag2 = _repository.GetById(2);
            
            _repository.Save(editableTag);

            Assert.Throws<EntityAlreadyExistsException>(() => editableTag.Name = tag2.Name);
        }

        [Test]
        public void CouldSetAccountName_method_returns_true_if_account_name_could_be_added_to_repository()
        {
            Assert.IsTrue(_repository.CouldSetAccountTagName(_accountTag1, "Holidays"));
        }

        [Test]
        public void CouldSetAccountName_method_returns_false_if_account_name_is_a_duplicate()
        {
            Assert.IsFalse(_repository.CouldSetAccountTagName(_accountTag2, "Entertainment"));
        }

        [Test]
        public void can_add_a_new_account_tag_by_passing_a_string()
        {
            var newTag = _repository.AddNewAccountTag("Holiday Food");
            var newTagRetrieved = _repository.GetById(newTag.Id);

            Assert.AreEqual(14, newTag.Id);
            Assert.AreEqual("Holiday Food", newTag.Name);
            Assert.AreEqual(14, _repository.GetAll().Count());
            var accountTagInts = _repository.GetAll().Select(x => x.Id);
            Assert.IsTrue(accountTagInts.Contains(newTag.Id));
            Assert.AreEqual(newTag.Id, newTagRetrieved.Id);
            Assert.AreEqual(newTag.Name, newTagRetrieved.Name);
        }

        [Test]
        public void adding_an_AccountTag_with_an_existing_name_just_returns_the_existing_AccountTag()
        {
            var tag = _repository.GetById(5);
            var duplicateTag = _repository.AddNewAccountTag(tag.Name);

            Assert.AreEqual(tag.Id, duplicateTag.Id);
            Assert.AreEqual(tag.Name, duplicateTag.Name);
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();

            Assert.AreEqual(TestData, output);
        }
    }
}