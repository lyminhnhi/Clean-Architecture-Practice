namespace CodeLeap.Application.DTOs.User
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
    }
}
