using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Employeetime.Common.Responses;
using Employeetime.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Employeetime.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
          [Table("employeetime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
          [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
           ILogger log)
        {
            log.LogInformation($"Recieved a new consolidation proceess, start at {DateTime.Now}");

            string filterTime = TableQuery.GenerateFilterConditionForBool("Consolidate", QueryComparisons.Equal, false);
            TableQuery<EmployeetimeEntity> queryTime = new TableQuery<EmployeetimeEntity>().Where(filterTime);
            TableQuerySegment<EmployeetimeEntity> unconsolidatedEntries = await timeTable.ExecuteQuerySegmentedAsync
            (queryTime, null);
            List<EmployeetimeEntity> OrderUnconsolidatedEntries = unconsolidatedEntries.OrderBy(x => x.Id).ThenBy(x
             => x.EntryDate).ToList();
            List<EmployeetimeEntity> listEmployeeConsolidate = GetListEmployeeConsolidate(OrderUnconsolidatedEntries);
            List<ConsolidatedEntity> consolidatedEntities = GetListConsolidate(listEmployeeConsolidate);


            foreach (ConsolidatedEntity item in consolidatedEntities)
            {
                string FilterIdConsolite = TableQuery.GenerateFilterConditionForInt("Id_Employee", QueryComparisons.Equal, (int)item.Id_Employee);
                string FilterDateConsolite = TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.Equal, GetDay());
                string FileterConsolidate = TableQuery.CombineFilters(FilterIdConsolite, TableOperators.And, FilterDateConsolite);
                TableQuery<ConsolidatedEntity> queryTime2 = new TableQuery<ConsolidatedEntity>().Where(FileterConsolidate);
                TableQuerySegment<ConsolidatedEntity> listConsolidateEntities = await consolidatedTable.ExecuteQuerySegmentedAsync(queryTime2, null);

                //listEmployeeConsolidate.Where(e=>e.Id==item.Id_Employee)

                if (listConsolidateEntities.Count() > 0)
                {
                    item.Min_Worked = item.Min_Worked + listConsolidateEntities.Results[0].Min_Worked;
                    TableOperation addOperation = TableOperation.Replace(item);
                    await consolidatedTable.ExecuteAsync(addOperation);

                }
                else
                {
                    TableOperation addOperation = TableOperation.Insert(item);
                    await consolidatedTable.ExecuteAsync(addOperation);

                }

            }

            foreach (EmployeetimeEntity item in listEmployeeConsolidate)
            {
                item.Consolidate = true;
                TableOperation.InsertOrReplace(item);
                TableOperation addOperation = TableOperation.Replace(item);
                await timeTable.ExecuteAsync(addOperation);
            }

        }
        private static int GetDiferentDate(DateTime finaldate, DateTime Inicialdate)

        {
            TimeSpan Diference = finaldate - Inicialdate;

            return (int)Diference.TotalMinutes;
        }
        private static List<ConsolidatedEntity> GetListConsolidate(List<EmployeetimeEntity> listEmployeeConsolidate)
        {
            List<ConsolidatedEntity> consolidatedEntities = new List<ConsolidatedEntity>();

            int entry = 0;//0:entrada, 1:Salida
            for (int i = 0; i < listEmployeeConsolidate.Count; i++)
            {
                if (listEmployeeConsolidate.Count - 1 == i)///Es ultimo registro
                {
                    if (listEmployeeConsolidate[i].Type == entry)
                    {
                        ///no hago nada
                    }
                    else//Salida
                    {
                        DateTime finaldate = listEmployeeConsolidate[i].EntryDate;
                        DateTime Inicialdate = listEmployeeConsolidate[i].EntryDate;
                        int DiferenceMinutes = GetDiferentDate(finaldate, Inicialdate);
                        ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity();
                        consolidatedEntity.Id_Employee = listEmployeeConsolidate[i].Id;
                        consolidatedEntity.Date = GetDay();
                        consolidatedEntity.Min_Worked = DiferenceMinutes;
                        consolidatedEntity.PartitionKey = "CONSOLIDATE";
                        consolidatedEntity.RowKey = Guid.NewGuid().ToString();
                        consolidatedEntities.Add(consolidatedEntity);

                    }
                }
                else if (listEmployeeConsolidate[i].Type == entry && listEmployeeConsolidate[i + 1] != null)
                {
                    DateTime finaldate = listEmployeeConsolidate[i + 1].EntryDate;
                    DateTime Inicialdate = listEmployeeConsolidate[i].EntryDate;
                    int DiferenceMinutes = GetDiferentDate(finaldate, Inicialdate);
                    ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity();
                    consolidatedEntity.Id_Employee = listEmployeeConsolidate[i].Id;
                    consolidatedEntity.Date = GetDay();
                    consolidatedEntity.Min_Worked = DiferenceMinutes;
                    consolidatedEntity.PartitionKey = "CONSOLIDATE";
                    consolidatedEntity.ETag = "*";
                    consolidatedEntity.RowKey = Guid.NewGuid().ToString();
                    consolidatedEntities.Add(consolidatedEntity);
                }
            }

            return consolidatedEntities;
        }

        private static DateTime GetDay()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }

        private static bool ValidateNextItem(List<EmployeetimeEntity> employeetimeEntities, int i)
        {
            try
            {
                return employeetimeEntities.Count > (i + 1);

            }
            catch (Exception)
            {
                return false;
            }
        }

        private static List<EmployeetimeEntity> GetListEmployeeConsolidate(List<EmployeetimeEntity> employeetimeEntities)
        {
            List<EmployeetimeEntity> ListConsolidate = new List<EmployeetimeEntity>();
            int Entry = 0;
            int Out = 1;

            for (int i = 0; i < employeetimeEntities.Count; i++)
            {
                int? IdEmpleado = employeetimeEntities[i].Id;
                int? Type = employeetimeEntities[i].Type;
                if (Type == Entry && ValidateNextItem(employeetimeEntities, i))
                {
                    if (employeetimeEntities[i + 1].Id == IdEmpleado && employeetimeEntities[i + 1].Type == Out)
                    {
                        ListConsolidate.Add(employeetimeEntities[i]);
                    }
                }
                else
                {
                    ListConsolidate.Add(employeetimeEntities[i]);
                }
            }
            return ListConsolidate;

        }
    }
}

