using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Inforigami.Regalo.Core.EventSourcing;
using Inforigami.Regalo.Interfaces;
using Newtonsoft.Json;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Inforigami.Regalo.SqlServer
{
    public class SqlServerEventStore : IEventStore, IDisposable
    {
        private readonly string _connectionName;

        public SqlServerEventStore(string connectionName)
        {
            if (connectionName == null) throw new ArgumentNullException("connectionName");
            _connectionName = connectionName;
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (newEvents == null) throw new ArgumentNullException("newEvents");
            Guid aggregateIdGuid;
            if (!Guid.TryParse(aggregateId, out aggregateIdGuid)) throw new ArgumentException(string.Format("\"{0}\" is not a valid Guid", aggregateId), "aggregateId");
            
            using (var transaction = GetTransaction())
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    if (expectedVersion == EventStreamVersion.NoStream)
                    {
                        InsertAggregateRow(aggregateIdGuid, newEvents, connection);
                    }
                    else
                    {
                        UpdateAggregateRow(aggregateIdGuid, expectedVersion, newEvents, connection);
                    }

                    InsertEvents(aggregateIdGuid, newEvents, connection);
                }

                transaction.Complete();
            }
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            throw new NotImplementedException();
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        private static TransactionScope GetTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions{IsolationLevel = IsolationLevel.ReadCommitted});
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
            var aggregateRootCommand = connection.CreateCommand();

            aggregateRootCommand.CommandType = CommandType.Text;
            aggregateRootCommand.CommandText = @"insert into AggregateRoot (Id, [Version]) values (@Id, @Version);";

            aggregateRootCommand.Parameters.AddWithValue("@Id", aggregateId);
            aggregateRootCommand.Parameters.AddWithValue("@Version", newEvents.Last().Version);

            aggregateRootCommand.ExecuteNonQuery();
        }

        private SqlConnection GetConnection()
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[_connectionName];

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
            return new JsonSerializerSettings { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.Auto };
        }
    }
}