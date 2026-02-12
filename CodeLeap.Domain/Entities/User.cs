using CodeLeap.Domain.Common;
using System.Text.RegularExpressions;

namespace CodeLeap.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public User() { }

        public User(string email, string passwordHash)
        {
            Email = email;
            PasswordHash = passwordHash;
        }

        public bool IsValidEmail()
        {
            if (string.IsNullOrEmpty(Email))
                return false;

            return Regex.IsMatch(Email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
