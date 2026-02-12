using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using CodeLeap.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(AppDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(RefreshToken token)
    {
        EnsureNotNull(token);

        _logger.LogInformation("Adding new refresh token for email: {Email}", token.Email);

        await _context.RefreshTokens.AddAsync(token);
        await SaveChangesAsync("Refresh token added successfully for email: {Email}", token.Email);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        _logger.LogDebug("Querying refresh token: {Token}", token);

        var entity = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (entity == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", token);
        }

        return entity;
    }

    public async Task RevokeAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        _logger.LogInformation("Revoking refresh token: {Token}", token);

        var entity = await GetByTokenAsync(token);

        if (entity == null)
        {
            _logger.LogWarning("Cannot revoke token - not found: {Token}", token);
            return;
        }

        entity.IsRevoked = true;

        await SaveChangesAsync("Refresh token revoked: {Token}", token);
    }

    public async Task RevokeAllByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        _logger.LogInformation("Revoking all refresh tokens for email: {Email}", email);

        var tokens = await _context.RefreshTokens
            .Where(x => x.Email == email && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await SaveChangesAsync("All refresh tokens revoked for email: {Email}", email);
    }

    private async Task SaveChangesAsync(string successMessage, params object[] args)
    {
        await _context.SaveChangesAsync();
        _logger.LogInformation(successMessage, args);
    }

    private static void EnsureNotNull(RefreshToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));
    }
}