using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using CodeLeap.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(User user)
        {
            EnsureNotNull(user);

            _logger.LogInformation("Adding new user with email: {Email}", user.Email);

            await _context.Users.AddAsync(user);
            await SaveChangesAsync("User added successfully: {Email}", user.Email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            _logger.LogDebug("Querying user by email: {Email}", email);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                _logger.LogWarning("User not found with email: {Email}", email);
            }
            else
            {
                _logger.LogInformation("User found with email: {Email}", email);
            }

            return user;
        }

        private async Task SaveChangesAsync(string successMessage, params object[] args)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation(successMessage, args);
        }

        private static void EnsureNotNull(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
        }
    }
}
