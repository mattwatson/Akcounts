using System;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;

namespace Akcounts.Domain.Objects
{
    public sealed class AccountTag : EntityIdentifiedByInt<AccountTag>, IDomainObject
    {
        public AccountTag(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public event EventHandler<NameChangeEventArgs> NameChanged;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                var args = new NameChangeEventArgs(value);
                if (NameChanged != null) NameChanged(this, args);
                _name = value;
            }
        }

        public bool IsValid
        {
            get
            {
                return ValidateId()
                    && ValidateName();
            }
        }

        private bool ValidateId()
        {
            return (Id != 0);
        }

        private bool ValidateName()
        {
            return !String.IsNullOrWhiteSpace(Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public XElement EmitXml()
        {
            return new XElement(
                    "tag",
                    new XAttribute("id", Id),
                    new XText(Name)
                    );
        }
    }
}
