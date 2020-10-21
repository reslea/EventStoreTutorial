using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class EventStoreUtilities
    {
        public static async Task<IEventStoreConnection> GetConnectionAsync()
        {
            var connectionString = "tcp://admin:changeit@localhost:1113";
            var conn = EventStoreConnection.Create(new Uri(connectionString));
            await conn.ConnectAsync();
            Console.WriteLine("connected");
            return conn;
        }

        public static EventData GetEventData(string eventType, object @event, object eventMetadata = null)
        {
            var jsonEvent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
            var jsonEventMetadata = eventMetadata == null
                ? null
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMetadata));

            return new EventData(Guid.NewGuid(), eventType, true, jsonEvent, jsonEventMetadata);
        }
    }
}
