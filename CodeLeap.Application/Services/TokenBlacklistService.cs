using CodeLeap.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CodeLeap.Application.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task BlacklistAsync(string jti, DateTime expiry)
        {
            var ttl = expiry - DateTime.UtcNow;
            _cache.Set(jti, true, ttl);
            return Task.CompletedTask;
        }

        public Task<bool> IsBlacklistedAsync(string jti)
        {
            return Task.FromResult(_cache.TryGetValue(jti, out _));
        }
    }
}
