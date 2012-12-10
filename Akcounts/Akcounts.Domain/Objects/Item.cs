using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akcounts.Domain
{
    public class Item : IdentityFieldProvider<Item>
    {
        public virtual Transaction TransactionId { get; protected set; }
        public virtual Account Source { get; protected set; }
        public virtual Account Destination { get; protected set; }
        public virtual Decimal Value { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsVerified { get; set; }

        public virtual string SourceName { get { return Source.Name; } }
        public virtual string DestinationName { get { return Destination.Name; } }
        public virtual string TransactionDate { get { return TransactionId.Date.ToLongDateString(); } }
        public virtual string TransactionDesc
        {
            get
            {
                string tempDesc = "";

                if (TransactionId != null)
                {
                    tempDesc = TransactionId.Description;
                }

                if (tempDesc != "" && Description != "")
                    tempDesc += ": ";

                tempDesc += Description;

                return tempDesc;

            }
        }

        public virtual bool TVerified
        {
            get { return TransactionId.IsVerified; }
            set { TransactionId.IsVerified = value; }
        }

        public virtual void SetTransaction(Transaction tran)
        {
            if (TransactionId != null) TransactionId.Items.Remove(this);
            TransactionId = tran;
            tran.Items.Add(this);
        }

        public virtual void SetSource(Account account)
        {
            if (account.Type != null)
            {
                if (account.Type.IsSource == false) throw new ItemInvalidSourceException();
            }
            if (account != null)
            {
                if (account == Destination) throw new ItemSourceEqualDestinationException();
            }

            if (Source != null) Source.ItemsSource.Remove(this);
            Source = account;
            Source.ItemsSource.Add(this);
        }

        public virtual void SetDestination(Account account)
        {
            if (account.Type != null)
            {
                if (account.Type.IsDestination == false) throw new ItemInvalidDestinationException();
            }
            if (account != null)
            {
                if (account == Source) throw new ItemSourceEqualDestinationException();
            }

            if (Destination != null) Destination.ItemsDestination.Remove(this);
            Destination = account;
            Destination.ItemsDestination.Add(this);
        }


        public virtual void SetSourceLazy(Account account)
        {
            if (account.Type != null)
            {
                if (account.Type.IsSource == false) throw new ItemInvalidSourceException();
            }
            if (account != null)
            {
                if (account == Destination) throw new ItemSourceEqualDestinationException();
            }

            if (Source != null) Source.ItemsSource.Remove(this);
            Source = account;
        }

        public virtual void SetDestinationLazy(Account account)
        {
            if (account.Type != null)
            {
                if (account.Type.IsDestination == false) throw new ItemInvalidDestinationException();
            }
            if (account != null)
            {
                if (account == Source) throw new ItemSourceEqualDestinationException();
            }

            if (Destination != null) Destination.ItemsDestination.Remove(this);
            Destination = account;
        }

    }
}
