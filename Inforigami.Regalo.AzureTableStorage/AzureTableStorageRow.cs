using System;
using Azure;
using Azure.Data.Tables;

namespace Inforigami.Regalo.AzureTableStorage
{
    public class AzureTableStorageAggregateRow : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public int Version { get; set; }
    }

    public class AzureTableStorageAggregateEventRow : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public int Version { get; set; }
        public string EventJson { get; set; }
    }
}