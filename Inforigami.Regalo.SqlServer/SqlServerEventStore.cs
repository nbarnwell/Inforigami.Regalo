using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

using Newtonsoft.Json;

namespace Inforigami.Regalo.SqlServer
{
    public class SqlServerEventStore : IEventStore, IDisposable
    {
        private readonly Func<ISqlSession> _sqlSessionFactory;
        private readonly ILogger _logger;

        public SqlServerEventStore(string connectionString, ILogger logger)
            : this(() => new TransientSqlSession(connectionString), logger)
        {
        }

        public SqlServerEventStore(Func<ISqlSession> sqlSessionFactory, ILogger logger)
        {
            _sqlSessionFactory = sqlSessionFactory ?? throw new ArgumentNullException("sqlSessionFactory");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }

        public void Save<T>(string eventStreamId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (newEvents == null) throw new ArgumentNullException("newEvents");

            using (var session = _sqlSessionFactory.Invoke())
            {
                if (expectedVersion == EntityVersion.New)
                {
                    InsertEventStreamRow(eventStreamId, newEvents, session);
                }
                else
                {
                    UpdateEventStreamRow(eventStreamId, expectedVersion, newEvents, session);
                }

                InsertEvents(eventStreamId, newEvents, session);

                session.Complete();
            }
        }

        public EventStream<T> Load<T>(string eventStreamId)
        {
            return Load<T>(eventStreamId, EntityVersion.Latest);
        }

        public EventStream<T> Load<T>(string eventStreamId, int version)
        {
            if (string.IsNullOrWhiteSpace(eventStreamId)) throw new ArgumentException("An event stream ID is required", "eventStreamId");

            if (version == EntityVersion.New)
            {
                throw new ArgumentOutOfRangeException("version", "By definition you cannot load a stream when specifying the EntityVersion.New (-1) value.");
            }

            _logger.Debug(this, "Loading " + typeof(T) + " version " + EntityVersion.GetName(version) + " from stream " + eventStreamId);

            using (var session = _sqlSessionFactory.Invoke())
            {
                var command = session.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = @"select * from EventStreamEvent where EventStreamId = @eventStreamId and Version <= @Version order by Version;";

                var eventStreamIdParameter = command.AddParameter("@EventStreamId", DbType.String, 1024);
                var versionParameter     = command.AddParameter("@Version", DbType.Int32);

                eventStreamIdParameter.Value = eventStreamId;
                versionParameter.Value = version == EntityVersion.Latest ? int.MaxValue : version;

                var events = new List<IEvent>();
                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        events.Add((IEvent)JsonConvert.DeserializeObject(reader.GetString(2), GetJsonSerialisationSettings()));
                    }
                }

                if (events.Count == 0)
                {
                    return null;
                }

                var result = new EventStream<T>(eventStreamId);
                result.Append(events);

                if (version != EntityVersion.Latest && result.GetVersion() != version)
                {
                    var exception = new ArgumentOutOfRangeException("version", version, string.Format("Event for version {0} could not be found for stream {1}", version, eventStreamId));
                    exception.Data.Add("Existing stream", events);
                    throw exception;
                }

                session.Complete();

                return result;
            }
        }

        public void Delete(string eventStreamId, int version)
        {
            throw new NotImplementedException("Replaced with Delete<T>");
        }

        public void Delete<T>(string eventStreamId, int version)
        {
            using (var session = _sqlSessionFactory.Invoke())
            {
                session.Connection.Open();

                DeleteEvents(eventStreamId, session);
                DeleteEventStreamRow(eventStreamId, version, session);

                session.Complete();
            }
        }

        public void Flush()
        {
            // Nothing to do
        }

        public void Dispose()
        {
        }

        private void DeleteEvents(string eventStreamId, ISqlSession session)
        {
            var eventCommand = session.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"delete from EventStreamEvent where EventStreamId = @EventStreamId;";

            var eventStreamIdParameter = eventCommand.AddParameter("@EventStreamId", DbType.String, 1024);
            eventStreamIdParameter.Value = eventStreamId;

            eventCommand.ExecuteNonQuery();
        }

        private void DeleteEventStreamRow(string eventStreamId, int version, ISqlSession session)
        {
            var eventCommand = session.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"delete from EventStream where Id = @EventStreamId and [Version] = @Version;";

            var eventStreamIdParameter = eventCommand.AddParameter("@EventStreamId", DbType.String, 1024);
            var versionParameter       = eventCommand.AddParameter("@Version", DbType.Int32);

            eventStreamIdParameter.Value = eventStreamId;
            versionParameter.Value       = version;

            var rowsDeleted = eventCommand.ExecuteNonQuery();

            if (rowsDeleted == 0)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version", version));
                exception.Data.Add("Existing stream", eventStreamId);
                throw exception;
            }
        }

        private void InsertEvents(string eventStreamId, IEnumerable<IEvent> newEvents, ISqlSession session)
        {
            var eventCommand = session.CreateCommand();

            eventCommand.CommandType = CommandType.Text;
            eventCommand.CommandText = @"insert into EventStreamEvent (EventStreamId, [Version], Data) values (@EventStreamId, @Version, @Data);";

            var eventStreamIdParameter = eventCommand.AddParameter("@EventStreamId", DbType.String, 1024);
            var versionParameter       = eventCommand.AddParameter("@Version", DbType.Int32);
            var dataParameter          = eventCommand.AddParameter("@Data", DbType.String, -1);

            eventCommand.Prepare();

            foreach (var evt in newEvents)
            {
                eventStreamIdParameter.Value = eventStreamId;
                versionParameter.Value       = evt.Version;
                dataParameter.Value          = GetJson(evt);

                eventCommand.ExecuteNonQuery();
            }
        }

        private static void UpdateEventStreamRow(string eventStreamId, int expectedVersion, IEnumerable<IEvent> newEvents, ISqlSession session)
        {
            var eventStreamCommand = session.CreateCommand();

            eventStreamCommand.CommandType = CommandType.Text;
            eventStreamCommand.CommandText = @"update EventStream set Version = @Version where Id = @Id and Version = @ExpectedVersion;";

            eventStreamCommand.AddParameterWithValue("@Id", eventStreamId);
            eventStreamCommand.AddParameterWithValue("@Version", newEvents.Last().Version);
            eventStreamCommand.AddParameterWithValue("@ExpectedVersion", expectedVersion);

            int rowsUpdated = eventStreamCommand.ExecuteNonQuery();

            if (rowsUpdated == 0)
            {
                throw new EventStoreConcurrencyException(string.Format("Event stream {0} was not found at version {1}.", eventStreamId, expectedVersion));
            }
        }

        private static void InsertEventStreamRow(string eventStreamId, IEnumerable<IEvent> newEvents, ISqlSession session)
        {
            if (newEvents == null || !newEvents.Any())
            {
                return;
            }

            var eventStreamCommand = session.CreateCommand();

            eventStreamCommand.CommandType = CommandType.Text;
            eventStreamCommand.CommandText = @"insert into EventStream (Id, [Version]) values (@Id, @Version);";

            eventStreamCommand.AddParameterWithValue("@Id", eventStreamId);
            eventStreamCommand.AddParameterWithValue("@Version", newEvents.Last().Version);

            eventStreamCommand.ExecuteNonQuery();
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
