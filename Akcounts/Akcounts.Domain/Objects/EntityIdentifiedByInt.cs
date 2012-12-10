namespace Akcounts.Domain.Objects
{
    public abstract class EntityIdentifiedByInt<T>
        where T : EntityIdentifiedByInt<T>
    {
        private int? _oldHashCode;

        public virtual int Id { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as T;
            if (other == null)
                return false;

            // handle the case of comparing two NEW objects
            if (other.IsTransient && IsTransient) return ReferenceEquals(other, this);

            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
            if (IsNotTransient) return Id.GetHashCode();
            
            if (!_oldHashCode.HasValue) _oldHashCode = base.GetHashCode();
            
            return _oldHashCode.Value;
            // ReSharper restore NonReadonlyFieldInGetHashCode
            // ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
        }

        private bool IsNotTransient
        {
            get { return !(IsTransient); }
        }

        private bool IsTransient
        {
            get { return Equals(Id, 0); }
        }

        public static bool operator ==(EntityIdentifiedByInt<T> x, EntityIdentifiedByInt<T> y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(EntityIdentifiedByInt<T> x, EntityIdentifiedByInt<T> y)
        {
            return !(x == y);
        }
    }
}