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
            _logger.LogInformation("Adding new user with email: {Email}", user.Email);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User added successfully: {Email}", user.Email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
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
    }
}
