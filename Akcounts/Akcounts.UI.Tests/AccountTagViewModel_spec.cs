using System;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Tests.TestHelper;
using NUnit.Framework;
using NMock2;
using Akcounts.UI.ViewModel;
namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class AccountTagViewModel_spec
    {
        private Mockery _mocks;
        private IAccountTagRepository _mockAccountTagRepository;
        private PropertyChangedCounter _changeCounter;

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccountTagRepository = _mocks.NewMock<IAccountTagRepository>();
            _changeCounter = new PropertyChangedCounter();
        }

        [Test]
        public void can_create_new_AccountTagViewModel()
        {
            var tag = new AccountTag(1, "Entertainment");
            var vm = new AccountTagViewModel(tag, _mockAccountTagRepository);

            Assert.AreEqual(tag.Id, vm.TagId);
            Assert.AreEqual(tag.Name, vm.TagName);
        }

        [Test]
        public void exception_raised_if_you_create_new_AccountTagViewModel_with_null_accountTag()
        {
            Assert.Throws<ArgumentNullException>(() => new AccountTagViewModel(null, _mockAccountTagRepository));
        }

        [Test]
        public void exception_raised_if_you_create_new_AccountTagViewModel_with_null_accountTagRepository()
        {
            var tag = new AccountTag(1, "Entertainment");

            Assert.Throws<ArgumentNullException>(() => new AccountTagViewModel(tag, null));
        }

        [Test]
        public void TagId_property_returns_tags_id()
        {
            var tag = new AccountTag(122, "Misc expenses");
            var vm = new AccountTagViewModel(tag, _mockAccountTagRepository);

            Assert.AreEqual(122, vm.TagId);
        }

        [Test]
        public void changing_AccountTagName_to_valid_name_causes_AccountTag_to_be_updated()
        {
            var tag = new AccountTag(1, "Holiday");
            var vm = new AccountTagViewModel(tag, _mockAccountTagRepository);

            vm.PropertyChanged += (s, args) => _changeCounter.HandlePropertyChange(s, args);
            Expect.Once.On(_mockAccountTagRepository).Method("CouldSetAccountTagName").Will(Return.Value(true));
            Expect.Once.On(_mockAccountTagRepository).Method("Save").With(tag);

            vm.TagName = "Holidays";

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("TagName"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void changing_AccountTagName_to_same_name_does_not_cause_update()
        {
            var tag = new AccountTag(1, "Holiday");
            var vm = new AccountTagViewModel(tag, _mockAccountTagRepository);
            vm.PropertyChanged += _changeCounter.HandlePropertyChange;
            
            vm.TagName = "Holiday";

            Assert.AreEqual(0, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(0, _changeCounter.TotalChangeCount);
            Assert.AreEqual(0, _changeCounter.ChangeCount("TagName"));

            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void attempting_to_change_AccountTagName_to_a_name_that_would_cause_a_duplicate_in_repository_do_not_call_repository()
        {
            var tag = new AccountTag(1, "Holiday");
            var vm = new AccountTagViewModel(tag, _mockAccountTagRepository);

            vm.PropertyChanged += (s, args) => _changeCounter.HandlePropertyChange(s, args);
            Expect.Once.On(_mockAccountTagRepository).Method("CouldSetAccountTagName").Will(Return.Value(false));

            vm.TagName = "Duplicate Name";

            Assert.AreEqual(1, _changeCounter.NoOfPropertiesChanged);
            Assert.AreEqual(1, _changeCounter.TotalChangeCount);
            Assert.AreEqual(1, _changeCounter.ChangeCount("TagName"));
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}


      