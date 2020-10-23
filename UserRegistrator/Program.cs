using Core;
using EventStore.ClientAPI;
using EventStoreTutorial;
using Infrastructure;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace UserRegistrator
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var conn = await EventStoreUtilities.GetConnectionAsync();

            var streamName = $"$et-{RegisterUser.EventType}";
            await conn.SubscribeToStreamAsync(streamName, true, 
                (subsription, @event) => ProcessUserRegistration(conn, subsription, @event));

            Console.ReadLine();
        }

        private static async Task ProcessUserRegistration(IEventStoreConnection connection, EventStoreSubscription subscription, ResolvedEvent @event)
        {
            var userData = @event.Event.Data;
            var user = JsonConvert.DeserializeObject<RegisterUser>(Encoding.UTF8.GetString(userData));

            var streamName = @event.Event.EventStreamId;
            Console.Write($"registration. {streamName} ");
            Console.WriteLine($"Email: {user.Email}");

            var eventType = $"{RegisterUser.EventType}Success";

            var evt = EventStoreUtilities.GetEventData(eventType, user);

            await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, evt);
            Console.WriteLine($"Sent {eventType}");
        }
    }
}
