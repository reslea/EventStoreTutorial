using System;

namespace Core
{
    public class RegisterUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public static string EventType = nameof(RegisterUser);

        public RegisterUser(Guid userId, string userEmail)
        {
            Id = userId;
            Email = userEmail;
        }
    }
}
