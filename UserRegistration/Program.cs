using Core;
using EventStore.ClientAPI;
using EventStoreTutorial;
using Infrastructure;
using System;
using System.Threading.Tasks;

namespace UserRegistration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connection = await EventStoreUtilities.GetConnectionAsync();

            while(true)
            {
                Console.Write("enter new user email: ");
                var userEmail = Console.ReadLine();
                var userId = Guid.NewGuid();

                var streamName = $"user-{userId}";
                var registerUser = new RegisterUser(userId, userEmail);

                var evt = EventStoreUtilities.GetEventData(RegisterUser.EventType, registerUser);

                await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, evt);

                Console.WriteLine($"event was added, userId: {userId}");
            }
        }
    }
}
