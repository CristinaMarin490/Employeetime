using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employeetime.Functions.Entities
{
    public class EmployeetimeEntity : TableEntity
    {
        public int Id { get; set; }

        public DateTime EntryDate { get; set; }

        public string Type { get; set; }

        public bool Consolidate { get; set; }
    }
}
