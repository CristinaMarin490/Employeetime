using Employeetime.Common.Models;
using Employeetime.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Employeetime.Text.Helpers
{
    public class TestFactory
    {
        public static EmployeetimeEntity GetEmployeetimeEntity()
        {
            return new EmployeetimeEntity
            {
                ETag = "*",
                PartitionKey = "EMPLOYEETIME",
                RowKey = Guid.NewGuid().ToString(),
                EntryDate = DateTime.UtcNow,
                Type = 1,
                Id = 1,
                Consolidate = false,
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid entryId, EmployeeTime entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{entryId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(Guid entryId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{entryId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(EmployeeTime entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }
        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static EmployeeTime GetEmployeedRequest()
        {
            return new EmployeeTime
            {
                EntryDate = DateTime.UtcNow,
                Consolidate = false,
                Id = 1,
                Type = "Tipo",
            };
        }
        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
