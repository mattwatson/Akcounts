using System;
using System.ComponentModel;
using System.Windows;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.View;
using Akcounts.UI.ViewModel;

namespace Akcounts.UI
{
    //TODO Test me!?
    public partial class App
    {
        private readonly MainWindow _window = new MainWindow();
        private IMainWindowViewModel _viewModel;

        private IAccountTagRepository _accountTagRepository;
        private IAccountRepository _accountRepository;
        private IJournalRepository _journalRepository;
        private ITemplateRepository _templateRepository;

        const string AccountTagDataPath = "Data/accountTags.xml";
        const string AccountDataPath = "Data/accounts.xml";
        const string JournalDataPath = "Data/journals.xml";
        const string TemplateDataPath = "Data/templates.xml";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitialiseRepositories();
            InitialiseViewModelAndView();
        }

        private void InitialiseRepositories()
        {
            _accountTagRepository = new AccountTagRepository(AccountTagDataPath);
            _accountRepository = new AccountRepository(AccountDataPath, _accountTagRepository);
            _journalRepository = new JournalRepository(JournalDataPath, _accountRepository);
            _templateRepository = new TemplateRepository(TemplateDataPath, _accountRepository);
        }

        private void InitialiseViewModelAndView()
        {
            _viewModel = new MainWindowViewModel(_accountRepository, _accountTagRepository, _journalRepository, _templateRepository);

            WireUpCloseEventHandler();

            _window.DataContext = _viewModel;
            _window.Show();
        }

        private void WireUpCloseEventHandler()
        {
            EventHandler handler = (sender, args) => _window.Close();
            _viewModel.RequestClose += handler;

            CancelEventHandler handler2 = (sender, args) =>
            {
                if (!_viewModel.IsSavePending) return;

                bool cancelQuit = ShowQuitConfirmation();
                if (cancelQuit) args.Cancel = true;
            };

            _window.Closing += handler2;
        }

        private bool ShowQuitConfirmation()
        {
            var result = MessageBox.Show("Would you like to save before closing?", 
                "Quit Confirmation", 
                MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return true;
            }

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.SaveCommand.Execute(null);
            }

            return false;
        }
    }
}
