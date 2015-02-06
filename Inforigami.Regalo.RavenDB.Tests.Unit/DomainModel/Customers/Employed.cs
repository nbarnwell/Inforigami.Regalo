using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.RavenDB.Tests.Unit.DomainModel.Customers
{
    public class Employed : Event
    {
        public Guid EmployeeId { get; private set; }
        public DateTime StartDate { get; private set; }

        public Employed(Guid employeeId, DateTime startDate)
        {
            EmployeeId = employeeId;
            StartDate = startDate;
        }

        public bool Equals(Employed other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.EmployeeId.Equals(EmployeeId) && other.StartDate.Equals(StartDate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Employed)) return false;
            return Equals((Employed)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EmployeeId.GetHashCode()*397) ^ StartDate.GetHashCode();
            }
        }
    }
}
