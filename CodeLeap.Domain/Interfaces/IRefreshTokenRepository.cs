using CodeLeap.Domain.Entities;

namespace CodeLeap.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken token);
        Task RevokeAsync(string token);
        Task RevokeAllByEmailAsync(string email);
    }
}
