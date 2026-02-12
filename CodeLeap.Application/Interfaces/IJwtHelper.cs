namespace CodeLeap.Application.Interfaces
{
    public interface IJwtHelper
    {
        string GenerateToken(string email);
        string GenerateRefreshToken();
    }
}
