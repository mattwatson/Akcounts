using System;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace Akcounts.Domain
{
    public class AccountCategory : IdentityFieldProvider<AccountCategory>
    {
        public virtual string Name { get; set; }
        public virtual string Colour { get; set; }
        public virtual bool IsValid { get; set; }
        private ISet<Account> accounts = new HashedSet<Account>();
        public virtual ISet<Account> Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }
        public virtual int AccountCount()
        {
            return Accounts.Count;
        }
    }
}
