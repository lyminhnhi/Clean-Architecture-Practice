namespace CodeLeap.Application.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task BlacklistAsync(string jti, DateTime expiry);
        Task<bool> IsBlacklistedAsync(string jti);
    }
}
