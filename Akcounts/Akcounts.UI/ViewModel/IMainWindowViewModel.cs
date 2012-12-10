using System;
using System.Windows.Input;
using Akcounts.Domain.Objects;

namespace Akcounts.UI.ViewModel
{
    public interface IMainWindowViewModel
    {
        void OpenExistingJournalScreen(Journal journal);
        ICommand SaveCommand { get; }
        bool IsSavePending { get; }
        event EventHandler RequestClose;
    }
}