using System;

namespace Edit.AzureTableStorage.IntegrationTests
{
    public class EventTwo : IEvent
    {
        public EventTwo(string value)
        {
            Id = Guid.NewGuid();
            ValueTwo = value;
        }

        public Guid Id { get; set; }
        public string ValueTwo { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EventTwo) obj);
        }

        protected bool Equals(EventTwo other)
        {
            return Id.Equals(other.Id) && string.Equals(ValueTwo, other.ValueTwo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (ValueTwo != null ? ValueTwo.GetHashCode() : 0);
            }
        }
    }
}