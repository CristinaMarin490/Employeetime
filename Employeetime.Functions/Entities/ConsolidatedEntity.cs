using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Employeetime.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int? Id_Employee { get; set; }

        public DateTime Date { get; set; }

        public int Min_Worked { get; set; }
    }
}
