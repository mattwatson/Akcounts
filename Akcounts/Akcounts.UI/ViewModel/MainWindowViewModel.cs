using System;
using System.Windows.Input;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;

namespace Akcounts.UI.ViewModel
{
    public class MainWindowViewModel : WorkspaceViewModel, IMainWindowViewModel
    {
        private readonly IAccountTagRepository _accountTagRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IJournalRepository _journalRepository;
        private readonly ITemplateRepository _templateRepository;

        ObservableCollection<WorkspaceViewModel> _workspaces;

        private readonly ScreenOpener _monthlyBreakdownScreenOpener;
        private readonly ScreenOpener _accountBrowserScreenOpener;
        private readonly ScreenOpener _accountMaintenanceScreenOpener;
        private readonly ScreenOpener _templateScreenOpener;

        public MainWindowViewModel(IAccountRepository accountRepository, IAccountTagRepository accountTagRepository, 
                                   IJournalRepository journalRepository, ITemplateRepository templateRepository)
        {
            _accountTagRepository = accountTagRepository;
            _accountRepository = accountRepository;
            _journalRepository = journalRepository;
            _templateRepository = templateRepository;
            
            _accountTagRepository.RepositoryModified += OnRepositoryModified;
            _accountRepository.RepositoryModified += OnRepositoryModified;
            _journalRepository.RepositoryModified += OnRepositoryModified;
            _templateRepository.RepositoryModified += OnRepositoryModified;

            _monthlyBreakdownScreenOpener = new ScreenOpener(OpenMonthlyBreakdownScreen);
            _accountBrowserScreenOpener = new ScreenOpener(OpenAccountBrowserScreen);
            _accountMaintenanceScreenOpener = new ScreenOpener(OpenAccountMaintenanceScreen);
            _templateScreenOpener = new ScreenOpener(OpenTemplateScreen);

            base.DisplayName = "Akcounts";
        }


        public ICommand OpenAccountBrowserScreenCommand
        {
            get { return _accountBrowserScreenOpener.OpenCommand; }
        }

        private void OpenAccountBrowserScreen(EventHandler closeEventHandler)
        {
            var vm = new AccountBrowserViewModel(_accountRepository, _accountTagRepository, this);
            AddVmToWorkSpacesAndDisplay(vm, closeEventHandler);
        }

        
        public ICommand OpenAccountMaintenanceScreenCommand
        {
            get { return _accountMaintenanceScreenOpener.OpenCommand; }
        }

        private void OpenAccountMaintenanceScreen(EventHandler closeEventHandler)
        {
            var vm = new AccountMaintenenceViewModel(_accountRepository, _accountTagRepository);
            AddVmToWorkSpacesAndDisplay(vm, closeEventHandler);
        }


        public ICommand OpenMonthlyBreakdownScreenCommand
        {
            get { return _monthlyBreakdownScreenOpener.OpenCommand; }
        }

        private void OpenMonthlyBreakdownScreen(EventHandler closeEventHandler)
        {
            var vm = new MonthlyBreakdownViewModel(_accountRepository);////, _templateRepository);
            AddVmToWorkSpacesAndDisplay(vm, closeEventHandler);
        }


        public ICommand OpenTemplateScreenCommand {
            get { return _templateScreenOpener.OpenCommand; }
        }

        private void OpenTemplateScreen(EventHandler closeEventHandler)
        {
            var vm = new TemplateViewModel(_templateRepository, _journalRepository, _accountRepository);
            AddVmToWorkSpacesAndDisplay(vm, closeEventHandler);
        }


        RelayCommand _openNewJournalScreen;
        public ICommand OpenNewJournalScreenCommand
        {
            get { return _openNewJournalScreen ?? (_openNewJournalScreen = new RelayCommand(execute => OpenNewJournalScreen())); }
        }

        private void OpenNewJournalScreen()
        {
            var newJournal = new Journal(DateTime.Today);
            var vm = new JournalViewModel(newJournal, _journalRepository, _accountRepository);
            AddVmToWorkSpacesAndDisplay(vm, null);
        }


        public void OpenExistingJournalScreen(Journal journal)
        {
            var vm = new JournalViewModel(journal, _journalRepository, _accountRepository);
            AddVmToWorkSpacesAndDisplay(vm, null);
            vm.RequestDelete += DeleteJournal;
        }
        

        private void AddVmToWorkSpacesAndDisplay(WorkspaceViewModel vm, EventHandler closeEventHandler)
        {
            Workspaces.Add(vm);
            SetActiveWorkspace(vm);
            if (closeEventHandler != null) vm.RequestClose += closeEventHandler;
            base.OnPropertyChanged("Workspaces");
        }
        
        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            var collectionView = CollectionViewSource.GetDefaultView(Workspaces);
            if (collectionView != null)
            {
                collectionView.MoveCurrentTo(workspace);
            }
        }

        RelayCommand _saveCommand;
        public ICommand SaveCommand
        {
            get {
                return _saveCommand ?? (_saveCommand = new RelayCommand(execute => OnRequestSave(), canExecute => IsSavePending));
            }
        }

        void OnRequestSave()
        {
            _accountTagRepository.WriteXmlFile("data\\AccountTags.xml");
            _accountRepository.WriteXmlFile("data\\Accounts.xml");
            _journalRepository.WriteXmlFile("data\\Journals.xml");
            _templateRepository.WriteXmlFile("data\\Template.xml");
            
            IsSavePending = false;
        }

        void OnRepositoryModified(object sender, EventArgs e)
        {
            IsSavePending = true;
        }

        public bool IsSavePending { get; private set; }

        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= OnWorkspaceRequestClose;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            var workspace = sender as WorkspaceViewModel;
            if (workspace == null) return;
            workspace.Dispose();
            Workspaces.Remove(workspace);
        }

        public void DeleteJournal(object sender, EventArgs e)
        {
            var vm = sender as JournalViewModel;
            if (vm == null) throw new ArgumentException("DeleteJournal() requires an JournalViewModel as a parameter");

            _journalRepository.Remove(vm.Journal);
        }
    }
}
