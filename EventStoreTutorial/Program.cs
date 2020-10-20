using EventStore.ClientAPI;
using EventStoreTutorial.Cardevents;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventStoreTutorial
{
    class Program
    {

        public static Card Card = new Card();

        static async Task Main(string[] args)
        {
            using var connection = await GetConnectionAsync();

            var eventData = GetEventData("Duplicate", new { Datetime = new DateTime(2020, 10, 19) });
            
            var stream = "card-75d7486b-fbbe-4b08-8aa9-22bbbbbbbbbb";

            var events = await connection.ReadStreamEventsForwardAsync(stream, 0, 100, false);

            Console.WriteLine($"Initial balance: {Card.CurrentAmount}");
            foreach (var evt in events.Events)
            {
                ProcessCardEvent(Card, evt.Event);
            }

            Console.WriteLine($"Current balance: {Card.CurrentAmount}");

            await connection.SubscribeToStreamAsync(stream, false, CardEventHandler);

            Console.ReadLine();
        }

        static void CardEventHandler(EventStoreSubscription subscription, ResolvedEvent evt)
        {
            ProcessCardEvent(Card, evt.Event);
        }

        static async Task<IEventStoreConnection> GetConnectionAsync()
        {
            var connectionString = "tcp://admin:changeit@localhost:1113";
            var conn = EventStoreConnection.Create(new Uri(connectionString));
            await conn.ConnectAsync();
            Console.WriteLine("connected");
            return conn;
        }

        static EventData GetEventData(string eventType, object @event, object eventMetadata = null)
        {
            var jsonEvent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
            var jsonEventMetadata = eventMetadata == null
                ? null
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMetadata));

            return new EventData(Guid.NewGuid(), eventType, true, jsonEvent, jsonEventMetadata);
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