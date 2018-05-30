using System;

namespace Project3.ATS
{
    class PhoneNumber : IEquatable<PhoneNumber>
    {
        public PhoneNumber(string phoneNumber)
        {
            this.Value = phoneNumber;
        }

        public string Value { get; }

        public bool Equals(PhoneNumber other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PhoneNumber)obj);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(PhoneNumber p1, PhoneNumber p2)
        {
            return p1 != null && p1.Equals(p2);
        }

        public static bool operator !=(PhoneNumber p1, PhoneNumber p2)
        {
            return !(p1 == p2);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
