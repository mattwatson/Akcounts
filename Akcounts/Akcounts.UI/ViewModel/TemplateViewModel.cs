using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{
    public class TemplateViewModel : WorkspaceViewModel
    {
        readonly ObservableCollection<JournalViewModel> _journalVMs;

        private readonly ITemplateRepository _templateRepository;
        private readonly IJournalRepository _journalRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly Template _template;
        private RelayCommand _addJournalCommand;
                
        public TemplateViewModel(ITemplateRepository templateRepository, IJournalRepository journalRepository, IAccountRepository accountRepository)
        {
            _templateRepository = templateRepository;
            _journalRepository = journalRepository;
            _accountRepository = accountRepository;
            
            var journals = _templateRepository.GetTemplateJournals();
            _template = templateRepository.GetAll().Single();

            _journalVMs = new ObservableCollection<JournalViewModel>();
            foreach (var journal in journals.OrderBy(x => x.Date))
                AddJournalToInternalCollection(journal);
        }

        private void AddJournalToInternalCollection(Journal journal)
        {
            var journalVM = new JournalViewModel(journal, _journalRepository, _accountRepository, false);
            journalVM.RequestDelete += DeleteJournal;

            _journalVMs.Add(journalVM);
        }

        public ObservableCollection<JournalViewModel> Journals
        {
            get { return _journalVMs; }
        }

        public ICommand AddJournalCommand
        {
            get { return _addJournalCommand ?? (_addJournalCommand = new RelayCommand(param => OnRequestAddJournal())); }
        }

        void OnRequestAddJournal()
        {
            var newJournal = new Journal(DateTime.Today);
            AddJournalToInternalCollection(newJournal);

            _template.AddJournal(newJournal);
            _templateRepository.Save(_template);
            base.OnPropertyChanged("Journals");
        }

        public void DeleteJournal(object sender, EventArgs e)
        {
            var vm = sender as JournalViewModel;
            if (vm == null) throw new ArgumentException("DeleteJournal() requires an JournalViewModel as a parameter");

            _journalVMs.Remove(vm);

            var journalToRemove = vm.Journal;
            _template.RemoveJournal(journalToRemove);
            _templateRepository.Save(_template);

            base.OnPropertyChanged("Journals");
        }
    }
}
