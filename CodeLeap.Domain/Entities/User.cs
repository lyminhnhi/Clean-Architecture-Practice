using CodeLeap.Domain.Common;

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
            return !string.IsNullOrEmpty(Email) && Email.Contains('@');
        }
    }
}
