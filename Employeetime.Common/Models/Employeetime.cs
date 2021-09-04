using System;

namespace Employeetime.Common.Models
{
    public class EmployeeTime
    {
        public int Id { get; set; }

        public DateTime EntryDate { get; set; }

        public string Type { get; set; }

        public bool Consolidate { get; set; }
    }
}
