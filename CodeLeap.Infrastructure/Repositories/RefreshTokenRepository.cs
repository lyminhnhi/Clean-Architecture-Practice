using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using CodeLeap.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RefreshTokenRepository> _logger;

        public RefreshTokenRepository(
            AppDbContext context,
            ILogger<RefreshTokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(RefreshToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            _logger.LogDebug("Adding refresh token for email {Email}", token.Email);

            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    !x.IsRevoked &&
                    x.ExpiryDate > DateTime.UtcNow);
        }

        public async Task RevokeAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            var entity = await _context.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    !x.IsRevoked);

            if (entity == null) return;

            entity.IsRevoked = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for email {Email}", entity.Email);
        }

        public async Task RevokeAllByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            var tokens = await _context.RefreshTokens
                .Where(x =>
                    x.Email == email &&
                    !x.IsRevoked)
                .ToListAsync();

            if (tokens.Count == 0) return;

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All refresh tokens revoked for email {Email}", email);
        }
    }
}