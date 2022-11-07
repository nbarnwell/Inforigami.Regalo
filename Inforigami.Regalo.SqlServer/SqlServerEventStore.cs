using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Inforigami.Regalo.SqlServer
{
    public class SqlServerEventStore : IEventStore, IDisposable
    {
        private readonly string _connectionName;
        private readonly ILogger _logger;

        public SqlServerEventStore(string connectionName, ILogger logger)
        {
            if (connectionName == null) throw new ArgumentNullException("connectionName");
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _connectionName = connectionName;
            _logger = logger;
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (newEvents == null) throw new ArgumentNullException("newEvents");
            Guid aggregateIdGuid;
            if (!Guid.TryParse(aggregateId, out aggregateIdGuid)) throw new ArgumentException(string.Format("\"{0}\" is not a valid Guid", aggregateId), "aggregateId");
            
            using (var transaction = GetTransaction())
            using (var connection = GetConnection())
            {
                connection.Open();

                if (expectedVersion == EntityVersion.New)
                {
                    InsertAggregateRow(aggregateIdGuid, newEvents, connection);
                }
                else
                {
                    UpdateAggregateRow(aggregateIdGuid, expectedVersion, newEvents, connection);
                }

                InsertEvents(aggregateIdGuid, newEvents, connection);

                transaction.Complete();
            }
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

            using (var transaction = GetTransaction())
            using (var connection = GetConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = @"select * from AggregateRootEvent where AggregateId = @aggregateId and Version <= @Version order by Version;";

                var aggregateIdParameter = command.Parameters.Add("@AggregateId", SqlDbType.UniqueIdentifier);
                var versionParameter     = command.Parameters.Add("@Version", SqlDbType.Int);

                aggregateIdParameter.Value = Guid.Parse(aggregateId);
                versionParameter.Value = version == EntityVersion.Latest ? int.MaxValue : version;

                var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                var events = new List<IEvent>();
                while (reader.Read())
                {
                    events.Add((IEvent)JsonConvert.DeserializeObject(reader.GetString(3), GetJsonSerialisationSettings()));
                }

                if (events.Count == 0)
                {
                    return null;
                }

                var result = new EventStream<T>(aggregateId);
                result.Append(events);

                if (version != EntityVersion.Latest && result.GetVersion() != version)
                {
                    var exception = new ArgumentOutOfRangeException("version", version, string.Format("Event for version {0} could not be found for stream {1}", version, aggregateId));
                    exception.Data.Add("Existing stream", events);
                    throw exception;
                }

                transaction.Complete();

                return result;
            }
        }

        public void Delete(string aggregateId, int version)
        {
            throw new NotImplementedException("Replaced with Delete<T>");
        }

        public void Delete<T>(string aggregateId, int version)
        {
            Guid aggregateIdGuid;
            if (!Guid.TryParse(aggregateId, out aggregateIdGuid))
            {
                throw new ArgumentException($"\"{aggregateId}\" is not a valid Guid", nameof(aggregateId));
            }

            using (var transaction = GetTransaction())
            using (var connection = GetConnection())
            {
                connection.Open();

                DeleteEvents(aggregateIdGuid, connection);
                DeleteAggregateRow(aggregateIdGuid, version, connection);

                transaction.Complete();
            }
        }

        public void Flush()
        {
            // Nothing to do
        }

        public void Dispose()
        {
        }

        private static TransactionScope GetTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions{IsolationLevel = IsolationLevel.ReadCommitted});
        }

        private void DeleteEvents(Guid aggregateId, SqlConnection connection)
        {
            var eventCommand = connection.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"delete from AggregateRootEvent where AggregateId = @AggregateId;";

            var aggregateIdParameter = eventCommand.Parameters.Add("@AggregateId", SqlDbType.UniqueIdentifier);
            aggregateIdParameter.Value = aggregateId;

            eventCommand.ExecuteNonQuery();
        }

        private void DeleteAggregateRow(Guid aggregateId, int version, SqlConnection connection)
        {
            var eventCommand = connection.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"delete from AggregateRoot where Id = @AggregateId and [Version] = @Version;";

            var aggregateIdParameter = eventCommand.Parameters.Add("@AggregateId", SqlDbType.UniqueIdentifier);
            var versionParameter     = eventCommand.Parameters.Add("@Version", SqlDbType.Int);

            aggregateIdParameter.Value = aggregateId;
            versionParameter.Value     = version;

            var rowsDeleted = eventCommand.ExecuteNonQuery();

            if (rowsDeleted == 0)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version", version));
                exception.Data.Add("Existing stream", aggregateId);
                throw exception;
            }
        }

        private void InsertEvents(Guid aggregateId, IEnumerable<IEvent> newEvents, SqlConnection connection)
        {
            var eventCommand = connection.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"insert into AggregateRootEvent (Id, AggregateId, [Version], Data) values (@Id, @AggregateId, @Version, @Data);";

            var idParameter          = eventCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            var aggregateIdParameter = eventCommand.Parameters.Add("@AggregateId", SqlDbType.UniqueIdentifier);
            var versionParameter     = eventCommand.Parameters.Add("@Version", SqlDbType.Int);
            var dataParameter        = eventCommand.Parameters.Add("@Data", SqlDbType.NVarChar, -1);

            eventCommand.Prepare();

            foreach (var evt in newEvents)
            {
                idParameter.Value          = evt.MessageId;
                aggregateIdParameter.Value = aggregateId;
                versionParameter.Value     = evt.Version;
                dataParameter.Value        = GetJson(evt);

                eventCommand.ExecuteNonQuery();
            }
        }

        private static void UpdateAggregateRow(Guid aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents, SqlConnection connection)
        {
            var aggregateRootCommand = connection.CreateCommand();

            aggregateRootCommand.CommandType = CommandType.Text;
            aggregateRootCommand.CommandText = @"update AggregateRoot set Version = @Version where Id = @Id and @Version = @ExpectedVersion;";

            aggregateRootCommand.Parameters.AddWithValue("@Id", aggregateId);
            aggregateRootCommand.Parameters.AddWithValue("@Version", newEvents.Last().Version);
            aggregateRootCommand.Parameters.AddWithValue("@ExpectedVersion", expectedVersion);

            int rowsUpdated = aggregateRootCommand.ExecuteNonQuery();

            if (rowsUpdated == 0)
            {
                throw new EventStoreConcurrencyException(string.Format("Aggregate root {0} was not found at version {1}.", aggregateId, expectedVersion));
            }
        }

        private static void InsertAggregateRow(Guid aggregateId, IEnumerable<IEvent> newEvents, SqlConnection connection)
        {
            if (newEvents == null || !newEvents.Any())
            {
                return;
            }

            var aggregateRootCommand = connection.CreateCommand();

            aggregateRootCommand.CommandType = CommandType.Text;
            aggregateRootCommand.CommandText = @"insert into AggregateRoot (Id, [Version]) values (@Id, @Version);";

            aggregateRootCommand.Parameters.AddWithValue("@Id", aggregateId);
            aggregateRootCommand.Parameters.AddWithValue("@Version", newEvents.Last().Version);

            aggregateRootCommand.ExecuteNonQuery();
        }

        private SqlConnection GetConnection()
        {
            var connectionStringSetting = System.Configuration.ConfigurationManager.ConnectionStrings[_connectionName];

            if (connectionStringSetting == null)
            {
                throw new InvalidOperationException(string.Format("There is no connection named {0}.", _connectionName));
            }

            if (connectionStringSetting.ProviderName.Equals("System.Data.SqlClient") == false)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Connection string {0} may not be correct as it is not using the 'System.Data.SqlClient' provider.",
                        _connectionName));
            }

            return new SqlConnection(connectionStringSetting.ConnectionString);
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
    }
}