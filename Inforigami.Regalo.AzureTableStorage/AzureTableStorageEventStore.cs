using System;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
using Newtonsoft.Json;

namespace Inforigami.Regalo.AzureTableStorage
{
    public class AzureTableStorageEventStore : IEventStore, IDisposable
    {
        private static readonly string AggregateHeaderRowKey = EntityVersion.New.ToString();
        private readonly string _azureStorageAccountName;
        private readonly string _azureStorageAccountKey;
        private readonly ILogger _logger;
        private readonly string _azureStorageAccountUri;

        public AzureTableStorageEventStore(string azureStorageAccountName, string azureStorageAccountKey, ILogger logger)
        {
            _azureStorageAccountName = azureStorageAccountName ?? throw new ArgumentNullException(nameof(azureStorageAccountName));
            _azureStorageAccountKey  = azureStorageAccountKey  ?? throw new ArgumentNullException(nameof(azureStorageAccountKey));
            _logger                  = logger                  ?? throw new ArgumentNullException(nameof(logger));

            _azureStorageAccountUri  = $"https://{azureStorageAccountName}.table.core.windows.net";
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (newEvents == null) throw new ArgumentNullException("newEvents");

            if (!newEvents.Any()) return;

            var actions = new List<TableTransactionAction>();

            var aggregateTable = GetTableClient<T>();

            var version = newEvents.Last().Version;
            actions.Add(
                new TableTransactionAction(
                    TableTransactionActionType.UpsertReplace,
                    new AzureTableStorageAggregateRow
                    {
                        PartitionKey = aggregateId,
                        RowKey       = AggregateHeaderRowKey,
                        ETag         = new ETag(expectedVersion.ToString()),
                        Timestamp    = DateTimeOffsetProvider.Now(),
                        Version      = version
                    }));

            actions.AddRange(
                newEvents.Select(
                             x =>
                                 new AzureTableStorageAggregateEventRow()
                                 {
                                     PartitionKey = aggregateId,
                                     RowKey       = x.Version.ToString(),
                                     Timestamp    = x.Timestamp,
                                     ETag         = new ETag(x.Version.ToString()),
                                     Version      = x.Version,
                                     EventJson    = GetJson(x)
                                 })
                         .Select(x => new TableTransactionAction(TableTransactionActionType.Add, x)));

            aggregateTable.SubmitTransaction(actions);
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            return Load<T>(aggregateId, EntityVersion.Latest);
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            if (string.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentException("An aggregate ID is required", "aggregateId");

            if (version == EntityVersion.New)
            {
                throw new ArgumentOutOfRangeException("version", "By definition you cannot load a stream when specifying the EntityVersion.New (-1) value.");
            }

            _logger.Debug(this, "Loading " + typeof(T) + " version " + EntityVersion.GetName(version) + " from stream " + aggregateId);

            var aggregateTable = GetTableClient<T>();

            var filter = $"PartitionKey eq '{aggregateId}' and RowKey ne '{AggregateHeaderRowKey}'";

            // It's a shame to load all rows and filter client-side, but the querying on RowKey
            // in Azure Table Storage is alphanumeric, so version "10" is "less than" version "2"...
            var events =
                aggregateTable.Query<AzureTableStorageAggregateEventRow>(filter)
                              .Select(x => new { Version = Convert.ToInt64(x.RowKey), Row = x })
                              .Where(x => version == EntityVersion.Latest || (0 <= x.Version && x.Version <= version))
                              .OrderBy(x => x.Version)
                              .Select(x => (IEvent)JsonConvert.DeserializeObject(x.Row.EventJson, GetJsonSerialisationSettings()))
                              .ToList();

            if (!events.Any())
            {
                return null;
            }

            var result = new EventStream<T>(aggregateId);
            result.Append(events);

            if (version != EntityVersion.Latest && result.GetVersion() != version)
            {
                var exception = new ArgumentOutOfRangeException("version", version,
                                                                string.Format(
                                                                    "Event for version {0} could not be found for stream {1}",
                                                                    version, aggregateId));
                exception.Data.Add("Existing stream", events);
                throw exception;
            }

            return result;
        }

        public void Delete(string aggregateId, int version)
        {
            throw new NotImplementedException("Replaced with Delete<T>");
        }

        public void Delete<T>(string aggregateId, int expectedVersion)
        {
            var table = GetTableClient<T>();

            var actions = new List<TableTransactionAction>();

            actions.Add(
                new TableTransactionAction(
                    TableTransactionActionType.Delete,
                    new TableEntity(aggregateId, AggregateHeaderRowKey)
                    {
                        ETag = new ETag(expectedVersion.ToString())
                    }));

            actions.AddRange(
                Enumerable.Range(EntityVersion.New, expectedVersion)
                          .Select(x => new TableTransactionAction(TableTransactionActionType.Delete, new TableEntity(aggregateId, x.ToString()))));

            table.SubmitTransaction(actions);
        }

        public void Flush()
        {
            // Not applicable
        }

        public void Dispose()
        {
            // Not applicable
        }

        private string GetJson(IEvent evt)
        {
            var json = JsonConvert.SerializeObject(evt, GetJsonSerialisationSettings());
            return json;
        }

        private JsonSerializerSettings GetJsonSerialisationSettings()
        {
            return new JsonSerializerSettings { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.All };
        }

        private TableClient GetTableClient<T>()
        {
            var aggregateTableName = typeof(T).Name;
            var aggregateTable =
                new TableClient(
                    new Uri(_azureStorageAccountUri),
                    aggregateTableName,
                    new TableSharedKeyCredential(
                        _azureStorageAccountName,
                        _azureStorageAccountKey));
            aggregateTable.CreateIfNotExists();
            return aggregateTable;
        }
    }
}
