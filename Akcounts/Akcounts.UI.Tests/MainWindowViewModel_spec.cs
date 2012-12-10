using System;
using System.Collections.Generic;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.ViewModel;
using NMock2;
using NUnit.Framework;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class MainWindowViewModel_spec
    {
        private Mockery _mocks;
        private IAccountTagRepository _mockAccoutTagRepository;
        private IAccountRepository _mockAccoutRepository;
        private IJournalRepository _mockJournalRepository;
        private ITemplateRepository _mockTemplateRepository;
        private MainWindowViewModel _mainWindowViewModel;

        [SetUp]
        public void SetUp()
        {
            _mocks = new Mockery();
            _mockAccoutTagRepository = _mocks.NewMock<IAccountTagRepository>();
            _mockAccoutRepository = _mocks.NewMock<IAccountRepository>();
            _mockJournalRepository = _mocks.NewMock<IJournalRepository>();
            _mockTemplateRepository = _mocks.NewMock<ITemplateRepository>();

            Expect.Once.On(_mockAccoutRepository).EventAdd("RepositoryModified", NMock2.Is.Anything);
            Expect.Once.On(_mockAccoutTagRepository).EventAdd("RepositoryModified", NMock2.Is.Anything);
            Expect.Once.On(_mockJournalRepository).EventAdd("RepositoryModified", NMock2.Is.Anything);
            Expect.Once.On(_mockTemplateRepository).EventAdd("RepositoryModified", NMock2.Is.Anything);

            _mainWindowViewModel = new MainWindowViewModel(_mockAccoutRepository, _mockAccoutTagRepository, _mockJournalRepository, _mockTemplateRepository);
        }

        [Test]
        public void repositories_are_queried_when_MainWindowViewModel_is_created()
        {
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void IsSavePending_is_false_after_loading_repositories()
        {
            Assert.IsFalse(_mainWindowViewModel.IsSavePending);
            Assert.IsFalse(_mainWindowViewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void IsSavePending_is_true_after_AccountRepository_modified()
        {
            Fire.Event("RepositoryModified").On(_mockAccoutRepository).With(this, EventArgs.Empty);
            Assert.IsTrue(_mainWindowViewModel.IsSavePending);
            Assert.IsTrue(_mainWindowViewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void IsSavePending_is_true_after_AccountTagrepository_modified()
        {
            Fire.Event("RepositoryModified").On(_mockAccoutTagRepository).With(this, EventArgs.Empty);
            Assert.IsTrue(_mainWindowViewModel.IsSavePending);
            Assert.IsTrue(_mainWindowViewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void IsSavePending_is_false_after_save_is_complete()
        {
            Fire.Event("RepositoryModified").On(_mockAccoutRepository).With(this, EventArgs.Empty);

            Expect.Once.On(_mockAccoutRepository).Method("WriteXmlFile");
            Expect.Once.On(_mockAccoutTagRepository).Method("WriteXmlFile");
            Expect.Once.On(_mockJournalRepository).Method("WriteXmlFile");
            Expect.Once.On(_mockTemplateRepository).Method("WriteXmlFile");

            _mainWindowViewModel.SaveCommand.Execute(null);

            Assert.IsFalse(_mainWindowViewModel.IsSavePending);
            Assert.IsFalse(_mainWindowViewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void OpenAccountMaintenanceScreenCommand_can_be_executed_after_MainWindow_is_created()
        {
            Assert.IsTrue(_mainWindowViewModel.OpenAccountMaintenanceScreenCommand.CanExecute(null));
        }

        [Test]
        public void can_open_accounts_screen_using_OpenAccountMaintenanceScreenCommand_command()
        {
            Expect.Once.On(_mockAccoutTagRepository).Method("GetAll").Will(Return.Value(new List<AccountTag>()));
            Expect.Once.On(_mockAccoutRepository).Method("GetAll").Will(Return.Value(new List<Account>()));
            
            _mainWindowViewModel.OpenAccountMaintenanceScreenCommand.Execute(null);

            var accountMaintenanceScreen = _mainWindowViewModel.Workspaces[0];
            Assert.IsNotNull(accountMaintenanceScreen);
        }
        
        [Test]
        public void OpenAccountMaintenanceScreenCommand_cannot_be_executed_twice_without_closing_screen()
        {
            Expect.Once.On(_mockAccoutTagRepository).Method("GetAll").Will(Return.Value(new List<AccountTag>()));
            Expect.Once.On(_mockAccoutRepository).Method("GetAll").Will(Return.Value(new List<Account>()));
            _mainWindowViewModel.OpenAccountMaintenanceScreenCommand.Execute(null);
            Assert.IsFalse(_mainWindowViewModel.OpenAccountMaintenanceScreenCommand.CanExecute(null));
        }

        [Test]
        public void OpenAccountMaintenanceScreenCommand_can_be_executed_again_after_closing_screen()
        {
            Expect.Once.On(_mockAccoutTagRepository).Method("GetAll").Will(Return.Value(new List<AccountTag>()));
            Expect.Once.On(_mockAccoutRepository).Method("GetAll").Will(Return.Value(new List<Account>()));
            
            Assert.IsTrue(_mainWindowViewModel.OpenAccountMaintenanceScreenCommand.CanExecute(null));
            _mainWindowViewModel.OpenAccountMaintenanceScreenCommand.Execute(null);

            var accountMaintenanceScreen = _mainWindowViewModel.Workspaces[0];
            accountMaintenanceScreen.CloseCommand.Execute(null);

            Assert.IsTrue(_mainWindowViewModel.OpenAccountMaintenanceScreenCommand.CanExecute(null));
        }

        [Test]
        public void AddNewJournalCommand_can_be_executed_after_MainWindow_is_created()
        {
            Assert.IsTrue(_mainWindowViewModel.OpenNewJournalScreenCommand.CanExecute(null));
        }
    }
}


      