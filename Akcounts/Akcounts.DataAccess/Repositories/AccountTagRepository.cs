using System.Linq;
using System.Xml.Linq;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;

namespace Akcounts.DataAccess.Repositories
{
    public class AccountTagRepository : Repository<AccountTag>, IAccountTagRepository
    {
        private int _maxId;
        
        //public AccountTagRepository(XElement accountTags)
        //{
        //    Initialise(accountTags);
        //}

        public AccountTagRepository()
        {
            InitialiseRepository("Data/accountTags.xml");
        }

        protected override sealed void Initialise(XElement xElement)
        {
            foreach (XElement tag in xElement.Elements())
            {
                var tagId = (int)tag.Attribute("id");
                var newTag = new AccountTag(tagId, tag.Value);
                Add(newTag);
                if (tagId > _maxId) _maxId = tagId;
            }
        }

        public AccountTag AddNewAccountTag(string name)
        {
            return GetByName(name) ?? CreateAndAddTag(name);
        }

        private AccountTag GetByName(string name)
        {
            return Entities.Values.FirstOrDefault(tag => tag.Name == name);
        }

        private AccountTag CreateAndAddTag(string name)
        {
            var newTag = new AccountTag(++_maxId, name);
            Add(newTag);

            return newTag;
        }

        protected override void Add(AccountTag tag)
        {
            if (NameAlreadyExists(tag.Name)) throw new EntityAlreadyExistsException();
            tag.NameChanged += ValidateNameChange;
            base.Add(tag);
        }

        private bool NameAlreadyExists(string name, int? excludedId = null)
        {
            return Entities
                .Where(tag => tag.Value.Name == name)
                .Any(tag => !excludedId.HasValue || excludedId != tag.Value.Id);
        }

        private void ValidateNameChange(object sender, NameChangeEventArgs args)
        {
            var tag = sender as AccountTag;
            if (tag != null && NameAlreadyExists(args.NewName, tag.Id)) throw new EntityAlreadyExistsException();
        }

        public bool CouldSetAccountTagName(AccountTag tag, string name)
        {
            return !NameAlreadyExists(name, tag.Id);
        }

        public override string EntityNames
        {
            get { return "accountTags"; }
        }
    }
}
