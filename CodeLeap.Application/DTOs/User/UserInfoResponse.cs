namespace CodeLeap.Application.DTOs.User
{
    public class UserInfoResponse
    {
        public required int Id { get; set; }
        public required string Email { get; set; } = string.Empty;
    }
}