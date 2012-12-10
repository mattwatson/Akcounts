using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akcounts.Domain
{
    public class IdentityFieldProvider<T>
        where T : IdentityFieldProvider<T>
    {
        private Guid _id;
        private int? _oldHashCode;

        public virtual Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public override bool Equals(object obj)
        {
            T other = obj as T;
            if (other == null)
                return false;

            // handle the case of comparing two NEW objects
            bool otherIsTransient = Equals(other.Id, Guid.Empty);
            bool thisIsTransient = Equals(Id, Guid.Empty);
            if (otherIsTransient && thisIsTransient)
                return ReferenceEquals(other, this);

            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            // Once we have a hash code we'll never change it
            if (_oldHashCode.HasValue)
                return _oldHashCode.Value;

            bool thisIsTransient = Equals(Id, Guid.Empty);

            // When this instance is transient, we use the base GetHashCode()
            // and remember it, so an instance can NEVER change its hash code.
            if (thisIsTransient)
            {
                _oldHashCode = base.GetHashCode();
                return _oldHashCode.Value;
            }
            return Id.GetHashCode();
        }

        public static bool operator ==(IdentityFieldProvider<T> x, IdentityFieldProvider<T> y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(IdentityFieldProvider<T> x, IdentityFieldProvider<T> y)
        {
            return !(x == y);
        }
    }
}
