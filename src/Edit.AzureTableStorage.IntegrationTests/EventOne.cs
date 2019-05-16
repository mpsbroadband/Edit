using System;

namespace Edit.AzureTableStorage.IntegrationTests
{
    public class EventOne : IEvent
    {
        public EventOne(string value)
        {
            Id = Guid.NewGuid();
            ValueOne = value;
        }

        public Guid Id { get; set; }
        public string ValueOne { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EventOne) obj);
        }

        protected bool Equals(EventOne other)
        {
            return Id.Equals(other.Id) && string.Equals(ValueOne, other.ValueOne);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (ValueOne != null ? ValueOne.GetHashCode() : 0);
            }
        }
    }
}