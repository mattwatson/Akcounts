using System;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using Akcounts.UI.Util;

namespace Akcounts.UI.ViewModel
{

    public class AccountTagViewModel : DeletableViewModel//, IDataErrorInfo
    {
        private readonly AccountTag _tag;
        private readonly IAccountTagRepository _repository;
 
        public AccountTagViewModel(AccountTag tag, IAccountTagRepository repository)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            if (repository == null) throw new ArgumentNullException("repository");

            _tag = tag;
            _repository = repository;
        }

        public int TagId
        {
            get { return _tag.Id; }
        }

        public string TagName
        {
            get { return _tag.Name; }
            set
            {
                if (value == _tag.Name) return;
                if (_repository.CouldSetAccountTagName(_tag, value))
                {
                    _tag.Name = value;
                    _repository.Save(_tag);
                }
                base.OnPropertyChanged("TagName");
            }
        }

        ////Untested
        ////string IDataErrorInfo.Error
        ////{
        ////    get {
        ////        return null;
        ////    }
        ////}

        ////string IDataErrorInfo.this[string propertyName]
        ////{
        ////    get
        ////    {
        ////        string error = null;

        ////        if (propertyName == "TagName")
        ////        {
        ////            if (String.IsNullOrWhiteSpace(TagName)) return "Tag must have a Name";
        ////        }
                
        ////        // Dirty the commands registered with CommandManager,
        ////        // such as our Save command, so that they are queried
        ////        // to see if they can execute now.
        ////        CommandManager.InvalidateRequerySuggested();

        ////        return error;
        ////    }
        ////}
    }
}
