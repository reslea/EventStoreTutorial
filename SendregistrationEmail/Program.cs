using Core;
using EventStore.ClientAPI;
using EventStoreTutorial;
using Infrastructure;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SendregistrationEmail
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var conn = await EventStoreUtilities.GetConnectionAsync();

            var streamName = "$ce-user";
            await conn.SubscribeToStreamAsync(streamName, true,
                (subsription, @event) => ProcessRegisteredUsers(conn, subsription, @event));

            Console.ReadLine();
        }

        private static async Task ProcessRegisteredUsers(IEventStoreConnection connection, EventStoreSubscription subsription, ResolvedEvent @event)
        {
            if (@event.Event.EventType != $"{RegisterUser.EventType}Success") return;

            var userData = @event.Event.Data;
            var user = JsonConvert.DeserializeObject<RegisterUser>(Encoding.UTF8.GetString(userData));

            var streamName = @event.Event.EventStreamId;
            Console.Write($"email was sent to: {user.Email} ");

            var eventType = $"EmailSent";

            var evt = new EventData(Guid.NewGuid(), eventType, true, null, null);

            await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, evt);
            Console.WriteLine($"Sent {eventType}");
        }
    }
}
