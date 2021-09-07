using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Employeetime.Text.Helpers
{
    public class MockCloudTablesEmployees : CloudTable
    {
        public MockCloudTablesEmployees(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTablesEmployees(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTablesEmployees(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetEmployeetimeEntity()
            });
        }
    }
}
