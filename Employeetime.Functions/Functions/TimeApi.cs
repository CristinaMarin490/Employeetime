using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Employeetime.Common.Models;
using Employeetime.Common.Responses;
using Employeetime.Functions.Entities;

namespace Employeetime.Functions.Functions
{
    public static class TimeApi
    {
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateEntry")] HttpRequest req,
           [Table("employeetime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)

        {
            log.LogInformation("Recieved a new Entry.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeTime CreateEntry = JsonConvert.DeserializeObject<EmployeeTime>(requestBody);

            if (CreateEntry == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The entry have a registration."
                });
            }
            if (CreateEntry != null && CreateEntry.Id == 0)
            {
                log.LogInformation($"id employee {CreateEntry.Id}");

                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "to employee a event, have to send id employee"
                });
            }
            EmployeetimeEntity employeetimeEntity = new EmployeetimeEntity
            {
                Id = CreateEntry.Id,
                EntryDate = CreateEntry.EntryDate,
                Type = Int16.Parse(CreateEntry.Type),
                ETag = "*",
                Consolidate = false,
                PartitionKey = "EMPLOYEETIME",
                RowKey = Guid.NewGuid().ToString(),
            };

            TableOperation addOperation = TableOperation.Insert(employeetimeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New Entry stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeetimeEntity
            });
        }

        [FunctionName(nameof(UpdateEntry))]
        public static async Task<IActionResult> UpdateEntry(
             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "UpdateEntry/{rowkey}")] HttpRequest req,
             [Table("employeetime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
             string rowKey,
               ILogger log)

        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeTime updateEntry = JsonConvert.DeserializeObject<EmployeeTime>(requestBody);

            log.LogInformation($"Update entry {rowKey}, received.");

            TableOperation findOperation = TableOperation.Retrieve<EmployeetimeEntity>("EMPLOYEETIME", rowKey);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "entry not found."
                });
            }

            EmployeetimeEntity employeetimeEntity = new EmployeetimeEntity
            {
                Id = updateEntry.Id,
                EntryDate = updateEntry.EntryDate,
                Type = Int16.Parse(updateEntry.Type),
                ETag = "*",
                Consolidate = false,
                PartitionKey = "EMPLOYEETIME",
                RowKey = rowKey,
            };

            TableOperation editOperation = TableOperation.Replace(employeetimeEntity);
            await timeTable.ExecuteAsync(editOperation);

            string message = "Edit Entry in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeetimeEntity
            });
        }

        [FunctionName(nameof(GetAllEntry))]
        public static async Task<IActionResult> GetAllEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllEntry")] HttpRequest req,
            [Table("employeetime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Get all Entry");

            TableQuery<EmployeetimeEntity> query = new TableQuery<EmployeetimeEntity>();
            TableQuerySegment<EmployeetimeEntity> entrys = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all Entrys";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entrys
            });

        }

        [FunctionName(nameof(GetAllEntryById))]
        public static IActionResult GetAllEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllEntryById/{rowkey}")] HttpRequest req,
            [Table("employeetime", "EMPLOYEETIME", "{rowkey}", Connection = "AzureWebJobsStorage")] EmployeetimeEntity employeetime,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"Get Entry by id: {rowKey}, received");

            if (employeetime == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "entry not found."
                });
            }
            
            string message = $" Entry {employeetime.RowKey} retrieved ";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeetime
            });

        }


        [FunctionName(nameof(DeleteregistrationID))]
        public static async Task<IActionResult> DeleteregistrationID(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteregistrationID")] HttpRequest req)

        {
            return null;
        }

    }







}

