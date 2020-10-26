using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using EventStoreTutorial.Cardevents;
using Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EventStoreTutorial
{
    class Program
    {

        public static Card Card = new Card();

        static async Task Main(string[] args)
        {
            using var connection = await EventStoreUtilities.GetConnectionAsync();
            var creds = connection.Settings.DefaultUserCredentials;
                        
            var stream = "card-75d7486b-fbbe-4b08-8aa9-bbbbbbbbbbbb";

            var query = await File.ReadAllTextAsync(@"Queries\specifiCardQuery.js");

            var logger = new ConsoleLogger();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 2113);
            var manager = new ProjectionsManager(logger, endpoint, TimeSpan.FromSeconds(30));

            //await manager.CreateOneTimeAsync(query, creds);

            //var listOneTime = await manager.ListOneTimeAsync(creds);

            var projectionName = "bbb-card balance";

            var result = await manager.GetResultAsync(projectionName, creds);
            Console.WriteLine(JObject.Parse(result)["balance"]);

            var prjectionStream = $"$projections-{projectionName}-result";
            await connection.SubscribeToStreamAsync(prjectionStream, false, (_, evt) =>
            {
                var subscribedResult = JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data));
                Console.WriteLine(subscribedResult["balance"]);
            });

            Console.ReadLine();
        }

        private static async Task SuscribeToPersistentData(IEventStoreConnection connection, string stream, string group)
        {
            Console.WriteLine("Connecting to persistent subscription...");
            await connection.ConnectToPersistentSubscriptionAsync(
                stream, 
                group, 
                CardEventPersistentSubscriptionHandler, 
                autoAck: false);
        }

        private static async Task RenewStateAndSupscribeToUpdates(IEventStoreConnection connection, string stream)
        {
            var events = await connection.ReadStreamEventsForwardAsync(stream, 0, 100, false);

            Console.WriteLine($"Initial balance: {Card.CurrentAmount}");
            foreach (var evt in events.Events)
            {
                ProcessCardEvent(Card, evt.Event);
            }

            Console.WriteLine($"Current balance: {Card.CurrentAmount}");

            await connection.SubscribeToStreamAsync(stream, false, CardEventSubscriptionHandler);
        }

        static void CardEventPersistentSubscriptionHandler(EventStorePersistentSubscriptionBase persistentSubscription, ResolvedEvent evt)
        {
            ProcessCardEvent(Card, evt.Event);
            persistentSubscription.Acknowledge(evt.Event.EventId);
        }

        static void CardEventSubscriptionHandler(EventStoreSubscription subscription, ResolvedEvent evt)
        {
            ProcessCardEvent(Card, evt.Event);
        }
        
        static void ProcessCardEvent(Card card, RecordedEvent evt)
        {
            var jsonData = Encoding.UTF8.GetString(evt.Data);

            if (CardEventType.Outcome.ToString() == evt.EventType)
            {
                var data = JsonConvert.DeserializeObject<OutcomeEvent>(jsonData);
                card.CurrentAmount -= data.Sum;
                Console.WriteLine($"Outcome: {data.Sum}");
            }

            if (CardEventType.Income.ToString() == evt.EventType)
            {
                var data = JsonConvert.DeserializeObject<IncomeEvent>(jsonData);
                card.CurrentAmount += data.Sum;
                Console.WriteLine($"Income: {data.Sum}");
            }
        }
    }
}